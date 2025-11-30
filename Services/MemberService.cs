using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<IEnumerable<Member>> GetAllMembersAsync()
    {
        return await _memberRepository.GetAllAsync();
    }

    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        return await _memberRepository.GetByIdAsync(id);
    }

    public async Task<Member?> GetMemberByEmailAsync(string email)
    {
        return await _memberRepository.GetByEmailAsync(email);
    }

    public async Task<Member> CreateMemberAsync(Member member)
    {
        return await _memberRepository.AddAsync(member);
    }

    public async Task<Member> UpdateMemberAsync(Member member)
    {
        await _memberRepository.UpdateAsync(member);
        return member;
    }

    public async Task DeleteMemberAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member != null)
        {
            await _memberRepository.DeleteAsync(member);
        }
    }
}

