using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(GymAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Appointment>> GetByMemberIdAsync(int memberId)
    {
        return await _dbSet
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Activity)
            .Include(a => a.GymCenter)
            .Where(a => a.MemberId == memberId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByTrainerIdAsync(int trainerId)
    {
        return await _dbSet
            .Where(a => a.TrainerId == trainerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByGymCenterIdAsync(int gymCenterId)
    {
        return await _dbSet
            .Where(a => a.GymCenterId == gymCenterId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetWithDetailsAsync()
    {
        return await _dbSet
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Activity)
            .Include(a => a.GymCenter)
            .ToListAsync();
    }

    public async Task<Appointment?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Activity)
            .Include(a => a.GymCenter)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status)
    {
        return await _dbSet
            .Where(a => a.Status == status)
            .ToListAsync();
    }
}

