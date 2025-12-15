using GymApp.Entities;

namespace GymApp.Services;

// Üye servisleri için temel CRUD imzalarını tanımlar.
public interface IMemberService
{
    Task<IEnumerable<Member>> GetAllMembersAsync();
    Task<Member?> GetMemberByIdAsync(int id);
    Task<Member?> GetMemberByEmailAsync(string email);
    Task<Member> CreateMemberAsync(Member member);
    Task<Member> UpdateMemberAsync(Member member);
    Task DeleteMemberAsync(int id);
}

