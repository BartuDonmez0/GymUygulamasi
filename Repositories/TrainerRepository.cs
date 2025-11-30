using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

public class TrainerRepository : Repository<Trainer>, ITrainerRepository
{
    public TrainerRepository(GymAppDbContext context) : base(context)
    {
    }

    public async Task<Trainer?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Email == email);
    }

    public async Task<IEnumerable<Trainer>> GetWithAppointmentsAsync()
    {
        return await _dbSet
            .Include(t => t.Appointments)
            .ToListAsync();
    }

    public async Task<IEnumerable<Trainer>> GetAllWithWorkingHoursAsync()
    {
        return await _dbSet
            .Include(t => t.WorkingHours)
            .Include(t => t.GymCenter)
            .Include(t => t.TrainerActivities)
                .ThenInclude(ta => ta.Activity)
            .ToListAsync();
    }

    public async Task<Trainer?> GetWithAppointmentsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Appointments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

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

