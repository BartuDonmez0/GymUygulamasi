using GymApp.Entities;
using GymApp.Repositories;
using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class GymCenterService : IGymCenterService
{
    private readonly IGymCenterRepository _gymCenterRepository;
    private readonly GymAppDbContext _context;

    public GymCenterService(IGymCenterRepository gymCenterRepository, GymAppDbContext context)
    {
        _gymCenterRepository = gymCenterRepository;
        _context = context;
    }

    public async Task<IEnumerable<GymCenter>> GetAllGymCentersAsync()
    {
        return await _gymCenterRepository.GetAllWithWorkingHoursAsync();
    }

    public async Task<GymCenter?> GetGymCenterByIdAsync(int id)
    {
        return await _gymCenterRepository.GetWithWorkingHoursAsync(id);
    }

    public async Task<GymCenter?> GetGymCenterWithDetailsAsync(int id)
    {
        return await _gymCenterRepository.GetFullDetailsAsync(id);
    }

    public async Task<GymCenter> CreateGymCenterAsync(GymCenter gymCenter)
    {
        return await _gymCenterRepository.AddAsync(gymCenter);
    }

    public async Task<GymCenter> UpdateGymCenterAsync(GymCenter gymCenter)
    {
        // Mevcut çalışma saatlerini sil
        var existingWorkingHours = await _context.GymCenterWorkingHours
            .Where(wh => wh.GymCenterId == gymCenter.Id)
            .ToListAsync();
        
        _context.GymCenterWorkingHours.RemoveRange(existingWorkingHours);
        
        // Yeni çalışma saatlerini ekle
        if (gymCenter.WorkingHoursList != null && gymCenter.WorkingHoursList.Any())
        {
            foreach (var workingHour in gymCenter.WorkingHoursList)
            {
                workingHour.GymCenterId = gymCenter.Id;
                workingHour.Id = 0; // Yeni kayıt olarak işaretle
            }
            await _context.GymCenterWorkingHours.AddRangeAsync(gymCenter.WorkingHoursList);
        }
        
        // GymCenter'ı güncelle
        await _gymCenterRepository.UpdateAsync(gymCenter);
        await _context.SaveChangesAsync();
        
        return gymCenter;
    }

    public async Task DeleteGymCenterAsync(int id)
    {
        var gymCenter = await _gymCenterRepository.GetByIdAsync(id);
        if (gymCenter != null)
        {
            await _gymCenterRepository.DeleteAsync(gymCenter);
        }
    }

    public async Task<IEnumerable<GymCenter>> GetFilteredGymCentersAsync(int? dayOfWeek, int? activityId)
    {
        return await _gymCenterRepository.GetFilteredAsync(dayOfWeek, activityId);
    }
}

