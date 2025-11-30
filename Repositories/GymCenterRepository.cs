using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

public class GymCenterRepository : Repository<GymCenter>, IGymCenterRepository
{
    public GymCenterRepository(GymAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GymCenter>> GetWithActivitiesAsync()
    {
        return await _dbSet
            .Include(g => g.Activities)
            .ToListAsync();
    }

    public async Task<GymCenter?> GetWithActivitiesAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Activities)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GymCenter?> GetWithPhotosAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Photos)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GymCenter?> GetFullDetailsAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Activities)
            .Include(g => g.Photos)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GymCenter?> GetWithWorkingHoursAsync(int id)
    {
        return await _dbSet
            .Include(g => g.WorkingHoursList)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<GymCenter>> GetAllWithWorkingHoursAsync()
    {
        return await _dbSet
            .Include(g => g.WorkingHoursList)
            .ToListAsync();
    }
}

