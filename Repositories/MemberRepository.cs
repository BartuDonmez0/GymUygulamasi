using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

public class MemberRepository : Repository<Member>, IMemberRepository
{
    public MemberRepository(GymAppDbContext context) : base(context)
    {
    }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.Email == email);
    }

    public async Task<Member?> GetByEmailAndPasswordAsync(string email, string password)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.Email == email && m.Password == password);
    }

    public async Task<Member?> GetWithAppointmentsAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Appointments)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member?> GetWithRecommendationsAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Recommendations)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}

