using GymApp.Entities;

namespace GymApp.Services;

public interface IMemberService
{
    Task<IEnumerable<Member>> GetAllMembersAsync();
    Task<Member?> GetMemberByIdAsync(int id);
    Task<Member?> GetMemberByEmailAsync(string email);
    Task<Member> CreateMemberAsync(Member member);
    Task<Member> UpdateMemberAsync(Member member);
    Task DeleteMemberAsync(int id);
}

