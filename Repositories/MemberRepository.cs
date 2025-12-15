using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

// Member tablosu için özel sorguları barındıran repository sınıfı.
public class MemberRepository : Repository<Member>, IMemberRepository
{
    // Constructor - DbContext'i base repository'ye iletir.
    public MemberRepository(GymAppDbContext context) : base(context)
    {
    }

    // E‑posta adresine göre tek bir üyeyi döndürür.
    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.Email == email);
    }

    // E‑posta ve şifreye göre tek bir üyeyi döndürür (giriş için).
    public async Task<Member?> GetByEmailAndPasswordAsync(string email, string password)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.Email == email && m.Password == password);
    }

    // Id'ye göre üyeyi randevuları ile birlikte döndürür.
    public async Task<Member?> GetWithAppointmentsAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Appointments)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // Id'ye göre üyeyi yapay zeka önerileri ile birlikte döndürür.
    public async Task<Member?> GetWithRecommendationsAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Recommendations)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}

