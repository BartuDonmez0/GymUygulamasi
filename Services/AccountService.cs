using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

public class AccountService : IAccountService
{
    private readonly IMemberRepository _memberRepository;
    private const string AdminEmail = "G231210561@sakarya.edu.tr";
    private const string AdminPassword = "sau";

    public AccountService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Member?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return null;

        return await _memberRepository.GetByEmailAndPasswordAsync(email, password);
    }

    public async Task<Member?> RegisterAsync(Member member)
    {
        // Admin email'i ile kayıt olamaz
        if (member.Email == AdminEmail)
            return null;

        // Email kontrolü
        var existingMember = await _memberRepository.GetByEmailAsync(member.Email);
        if (existingMember != null)
            return null;

        // Yeni üye oluştur
        member.RegistrationDate = DateTime.UtcNow;
        return await _memberRepository.AddAsync(member);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        var member = await _memberRepository.GetByEmailAsync(email);
        return member != null;
    }

    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        return await _memberRepository.GetByIdAsync(id);
    }

    public bool IsAdmin(string email, string password)
    {
        return email == AdminEmail && password == AdminPassword;
    }
}

