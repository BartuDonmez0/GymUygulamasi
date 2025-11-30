using GymApp.Entities;

namespace GymApp.Services;

public interface IAccountService
{
    Task<Member?> LoginAsync(string email, string password);
    Task<Member?> RegisterAsync(Member member);
    Task<bool> IsEmailExistsAsync(string email);
    Task<Member?> GetMemberByIdAsync(int id);
    bool IsAdmin(string email, string password);
    Task<bool> DeleteMemberAsync(int id);
}

