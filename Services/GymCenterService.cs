using GymApp.Entities;
using GymApp.Repositories;
using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

// Spor salonu (GymCenter) ile ilgili iş kurallarını yöneten servis.
public class GymCenterService : IGymCenterService
{
    private readonly IGymCenterRepository _gymCenterRepository;
    private readonly GymAppDbContext _context;

    // Constructor - repository ve DbContext bağımlılıklarını alır.
    public GymCenterService(IGymCenterRepository gymCenterRepository, GymAppDbContext context)
    {
        _gymCenterRepository = gymCenterRepository;
        _context = context;
    }

    // Tüm spor salonlarını çalışma saatleriyle birlikte döndürür.
    public async Task<IEnumerable<GymCenter>> GetAllGymCentersAsync()
    {
        return await _gymCenterRepository.GetAllWithWorkingHoursAsync();
    }

    // Id'ye göre tek bir spor salonunu çalışma saatleriyle birlikte döndürür.
    public async Task<GymCenter?> GetGymCenterByIdAsync(int id)
    {
        return await _gymCenterRepository.GetWithWorkingHoursAsync(id);
    }

    // Id'ye göre spor salonunu tüm detaylarıyla (aktiviteler, fotoğraflar vb.) döndürür.
    public async Task<GymCenter?> GetGymCenterWithDetailsAsync(int id)
    {
        return await _gymCenterRepository.GetFullDetailsAsync(id);
    }

    // Yeni spor salonu kaydı oluşturur.
    public async Task<GymCenter> CreateGymCenterAsync(GymCenter gymCenter)
    {
        return await _gymCenterRepository.AddAsync(gymCenter);
    }

    // Var olan spor salonu kaydını ve çalışma saatlerini günceller.
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

    // Id'ye göre spor salonunu siler (varsa).
    public async Task DeleteGymCenterAsync(int id)
    {
        var gymCenter = await _gymCenterRepository.GetByIdAsync(id);
        if (gymCenter != null)
        {
            await _gymCenterRepository.DeleteAsync(gymCenter);
        }
    }

    // Gün ve aktiviteye göre filtrelenmiş spor salonu listesi döndürür.
    public async Task<IEnumerable<GymCenter>> GetFilteredGymCentersAsync(int? dayOfWeek, int? activityId)
    {
        return await _gymCenterRepository.GetFilteredAsync(dayOfWeek, activityId);
    }
}

