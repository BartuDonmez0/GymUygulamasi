using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

/// <summary>
/// Account Service - Kullanıcı kimlik doğrulama ve yetkilendirme işlemlerini yönetir
/// Rol bazlı yetkilendirme: Admin ve Üye rolleri desteklenir
/// CRUD işlemleri: Create (Register), Read (Login, GetMemberById), Delete (DeleteMember)
/// </summary>
public class AccountService : IAccountService
{
    private readonly IMemberRepository _memberRepository;
    // Admin kullanıcı bilgileri - Rol bazlı yetkilendirme için sabit değerler
    private const string AdminEmail = "G231210561@sakarya.edu.tr";
    private const string AdminPassword = "sau";

    /// <summary>
    /// Constructor - Dependency injection ile repository'yi alır
    /// </summary>
    public AccountService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    /// <summary>
    /// LoginAsync - Kullanıcı giriş işlemini gerçekleştirir
    /// Read işlemi: Email ve password ile kullanıcı doğrulama
    /// </summary>
    /// <param name="email">Kullanıcı e-posta adresi</param>
    /// <param name="password">Kullanıcı şifresi</param>
    /// <returns>Giriş başarılıysa Member nesnesi, değilse null</returns>
    public async Task<Member?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return null;

        return await _memberRepository.GetByEmailAndPasswordAsync(email, password);
    }

    /// <summary>
    /// RegisterAsync - Yeni kullanıcı kaydı oluşturur
    /// Create işlemi: Yeni üye kaydı
    /// Server-side validation: Email tekrar kontrolü ve admin email kontrolü
    /// </summary>
    /// <param name="member">Kayıt olacak üye bilgileri</param>
    /// <returns>Kayıt başarılıysa Member nesnesi, değilse null</returns>
    public async Task<Member?> RegisterAsync(Member member)
    {
        // Admin email'i ile kayıt olamaz - Güvenlik kontrolü
        if (member.Email == AdminEmail)
            return null;

        // Email kontrolü - Aynı email ile kayıt olamaz
        var existingMember = await _memberRepository.GetByEmailAsync(member.Email);
        if (existingMember != null)
            return null;

        // Yeni üye oluştur - Kayıt tarihini otomatik set et
        member.RegistrationDate = DateTime.UtcNow;
        return await _memberRepository.AddAsync(member);
    }

    /// <summary>
    /// IsEmailExistsAsync - Email adresinin veritabanında olup olmadığını kontrol eder
    /// Validation: Email tekrar kontrolü için kullanılır
    /// </summary>
    /// <param name="email">Kontrol edilecek e-posta adresi</param>
    /// <returns>Email varsa true, yoksa false</returns>
    public async Task<bool> IsEmailExistsAsync(string email)
    {
        var member = await _memberRepository.GetByEmailAsync(email);
        return member != null;
    }

    /// <summary>
    /// GetMemberByIdAsync - ID'ye göre üye bilgilerini getirir
    /// Read işlemi: Belirli bir üyenin bilgilerini çeker
    /// </summary>
    /// <param name="id">Üye ID'si</param>
    /// <returns>Üye bulunursa Member nesnesi, bulunamazsa null</returns>
    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        return await _memberRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// IsAdmin - Kullanıcının admin olup olmadığını kontrol eder
    /// Rol bazlı yetkilendirme: Admin rolü kontrolü
    /// </summary>
    /// <param name="email">Kullanıcı e-posta adresi</param>
    /// <param name="password">Kullanıcı şifresi</param>
    /// <returns>Admin ise true, değilse false</returns>
    public bool IsAdmin(string email, string password)
    {
        return email == AdminEmail && password == AdminPassword;
    }

    /// <summary>
    /// DeleteMemberAsync - Üye profilini siler
    /// Delete işlemi: Üye kaydını veritabanından siler
    /// Authorization: Admin hesapları silinemez
    /// </summary>
    /// <param name="id">Silinecek üye ID'si</param>
    /// <returns>Silme başarılıysa true, değilse false</returns>
    public async Task<bool> DeleteMemberAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member == null)
            return false;

        // Admin silinemez
        if (member.Email == AdminEmail)
            return false;

        await _memberRepository.DeleteAsync(member);
        return true;
    }
}

