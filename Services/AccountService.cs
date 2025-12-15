using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

// Kullanıcı hesap işlemlerini (giriş, kayıt, profil silme vb.) yöneten servis.
public class AccountService : IAccountService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IRepository<User> _userRepository;

    // Admin kullanıcı bilgileri - rol bazlı yetkilendirme için sabit değerler
    private const string AdminEmail = "G231210561@sakarya.edu.tr";
    private const string AdminPassword = "sau";

    // Constructor - repository bağımlılıklarını alır.
    public AccountService(
        IMemberRepository memberRepository,
        IRepository<User> userRepository)
    {
        _memberRepository = memberRepository;
        _userRepository = userRepository;
    }

    // Kullanıcı giriş işlemini gerçekleştirir (email + şifre ile doğrulama).
    public async Task<Member?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return null;

        return await _memberRepository.GetByEmailAndPasswordAsync(email, password);
    }

    // Yeni üye kaydı oluşturur ve User tablosuna rol bilgisini ekler.
    public async Task<Member?> RegisterAsync(Member member)
    {
        // Admin email'i ile kayıt olamaz (sistem admin'i tekil olmalı)
        if (member.Email == AdminEmail)
            return null;

        // Email kontrolü - Aynı email ile kayıt olamaz
        var existingMember = await _memberRepository.GetByEmailAsync(member.Email);
        if (existingMember != null)
            return null;

        // Yeni üye oluştur - kayıt tarihini otomatik set et
        member.RegistrationDate = DateTime.UtcNow;

        // Önce veritabanına Member kaydını ekle (ID alınır)
        var createdMember = await _memberRepository.AddAsync(member);

        // User tablosunda rol bilgisini tut - Rol: User (kayıtlı kullanıcı)
        var user = new User
        {
            Email = createdMember.Email,
            Password = createdMember.Password,
            Role = "User", // Rol bazlı yetkilendirme için kayıtlı kullanıcı rolü
            CreatedDate = DateTime.UtcNow
        };

        user = await _userRepository.AddAsync(user);

        // Member ile User arasındaki ilişkiyi kur (UserId foreign key)
        createdMember.UserId = user.Id;
        await _memberRepository.UpdateAsync(createdMember);

        return createdMember;
    }

    // Email adresinin daha önce kullanılıp kullanılmadığını kontrol eder.
    public async Task<bool> IsEmailExistsAsync(string email)
    {
        var member = await _memberRepository.GetByEmailAsync(email);
        return member != null;
    }

    // ID'ye göre üye bilgilerini getirir.
    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        return await _memberRepository.GetByIdAsync(id);
    }

    // Verilen email ve şifrenin admin kullanıcısına ait olup olmadığını kontrol eder.
    public bool IsAdmin(string email, string password)
    {
        return email == AdminEmail && password == AdminPassword;
    }

    // Üye profilini ve ilişkili User kaydını siler (admin hesabı hariç).
    public async Task<bool> DeleteMemberAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member == null)
            return false;

        // Admin silinemez
        if (member.Email == AdminEmail)
            return false;

        // Önce ilişkili User kaydını sil (rol bilgisini de temizle)
        if (member.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(member.UserId.Value);
            if (user != null)
            {
                await _userRepository.DeleteAsync(user);
            }
        }

        await _memberRepository.DeleteAsync(member);
        return true;
    }
}

