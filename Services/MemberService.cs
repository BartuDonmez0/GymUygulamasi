using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

// Üye (Member) ile ilgili temel CRUD işlemlerini yöneten servis.
public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    // Constructor - member repository bağımlılığını alır.
    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    // Tüm üyeleri döndürür.
    public async Task<IEnumerable<Member>> GetAllMembersAsync()
    {
        return await _memberRepository.GetAllAsync();
    }

    // Id'ye göre tek bir üyeyi döndürür.
    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        return await _memberRepository.GetByIdAsync(id);
    }

    // E‑posta adresine göre üyeyi döndürür.
    public async Task<Member?> GetMemberByEmailAsync(string email)
    {
        return await _memberRepository.GetByEmailAsync(email);
    }

    // Yeni üye kaydı oluşturur.
    public async Task<Member> CreateMemberAsync(Member member)
    {
        return await _memberRepository.AddAsync(member);
    }

    // Var olan bir üyeyi günceller.
    public async Task<Member> UpdateMemberAsync(Member member)
    {
        await _memberRepository.UpdateAsync(member);
        return member;
    }

    // Id'ye göre üye kaydını siler (varsa).
    public async Task DeleteMemberAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member != null)
        {
            await _memberRepository.DeleteAsync(member);
        }
    }
}

