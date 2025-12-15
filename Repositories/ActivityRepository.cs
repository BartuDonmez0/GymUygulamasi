using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

// Activity tablosu için özel sorguları barındıran repository sınıfı.
public class ActivityRepository : Repository<Activity>, IActivityRepository
{
    // Constructor - DbContext'i base repository'ye iletir.
    public ActivityRepository(GymAppDbContext context) : base(context)
    {
    }

    // Belirli bir spor salonuna ait aktiviteleri döndürür.
    public async Task<IEnumerable<Activity>> GetByGymCenterIdAsync(int gymCenterId)
    {
        return await _dbSet
            .Where(a => a.GymCenterId == gymCenterId)
            .ToListAsync();
    }

    // Tüm aktiviteleri spor salonu bilgisi ile birlikte döndürür.
    public async Task<IEnumerable<Activity>> GetWithGymCenterAsync()
    {
        return await _dbSet
            .Include(a => a.GymCenter)
            .ToListAsync();
    }

    // Id'ye göre tek bir aktiviteyi spor salonu bilgisi ile birlikte döndürür.
    public async Task<Activity?> GetWithGymCenterAsync(int id)
    {
        return await _dbSet
            .Include(a => a.GymCenter)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}

