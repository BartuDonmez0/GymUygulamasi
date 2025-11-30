using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

public class ActivityRepository : Repository<Activity>, IActivityRepository
{
    public ActivityRepository(GymAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Activity>> GetByGymCenterIdAsync(int gymCenterId)
    {
        return await _dbSet
            .Where(a => a.GymCenterId == gymCenterId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetWithGymCenterAsync()
    {
        return await _dbSet
            .Include(a => a.GymCenter)
            .ToListAsync();
    }

    public async Task<Activity?> GetWithGymCenterAsync(int id)
    {
        return await _dbSet
            .Include(a => a.GymCenter)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}

