using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

// Trainer tablosu için özel sorguları barındıran repository sınıfı.
public class TrainerRepository : Repository<Trainer>, ITrainerRepository
{
    // Constructor - DbContext'i base repository'ye iletir.
    public TrainerRepository(GymAppDbContext context) : base(context)
    {
    }

    // E‑posta adresine göre tek bir antrenörü döndürür.
    public async Task<Trainer?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Email == email);
    }

    // Tüm antrenörleri randevuları ile birlikte döndürür.
    public async Task<IEnumerable<Trainer>> GetWithAppointmentsAsync()
    {
        return await _dbSet
            .Include(t => t.Appointments)
            .ToListAsync();
    }

    // Tüm antrenörleri çalışma saatleri, salon ve aktiviteleri ile birlikte döndürür.
    public async Task<IEnumerable<Trainer>> GetAllWithWorkingHoursAsync()
    {
        return await _dbSet
            .Include(t => t.WorkingHours)
            .Include(t => t.GymCenter)
            .Include(t => t.TrainerActivities)
                .ThenInclude(ta => ta.Activity)
            .ToListAsync();
    }

    // Id'ye göre tek bir antrenörü randevuları ile birlikte döndürür.
    public async Task<Trainer?> GetWithAppointmentsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Appointments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    // Id'ye göre tek bir antrenörü çalışma saatleri, salon ve aktiviteleri ile birlikte döndürür.
    public async Task<Trainer?> GetWithWorkingHoursAsync(int id)
    {
        return await _dbSet
            .Include(t => t.WorkingHours)
            .Include(t => t.GymCenter)
            .Include(t => t.TrainerActivities)
                .ThenInclude(ta => ta.Activity)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}

