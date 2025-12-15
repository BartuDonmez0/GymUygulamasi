using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

// GymCenter tablosu için özel sorguları barındıran repository sınıfı.
public class GymCenterRepository : Repository<GymCenter>, IGymCenterRepository
{
    // Constructor - DbContext'i base repository'ye iletir.
    public GymCenterRepository(GymAppDbContext context) : base(context)
    {
    }

    // Tüm spor salonlarını aktiviteleri ile birlikte döndürür.
    public async Task<IEnumerable<GymCenter>> GetWithActivitiesAsync()
    {
        return await _dbSet
            .Include(g => g.Activities)
            .ToListAsync();
    }

    // Id'ye göre tek bir spor salonunu aktiviteleri ile birlikte döndürür.
    public async Task<GymCenter?> GetWithActivitiesAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Activities)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    // Id'ye göre tek bir spor salonunu fotoğrafları ile birlikte döndürür.
    public async Task<GymCenter?> GetWithPhotosAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Photos)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    // Id'ye göre spor salonunu hem aktiviteleri hem fotoğrafları ile birlikte döndürür.
    public async Task<GymCenter?> GetFullDetailsAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Activities)
            .Include(g => g.Photos)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    // Id'ye göre spor salonunu çalışma saatleri ile birlikte döndürür.
    public async Task<GymCenter?> GetWithWorkingHoursAsync(int id)
    {
        return await _dbSet
            .Include(g => g.WorkingHoursList)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    // Tüm spor salonlarını çalışma saatleri ve aktiviteleri ile birlikte döndürür.
    public async Task<IEnumerable<GymCenter>> GetAllWithWorkingHoursAsync()
    {
        return await _dbSet
            .Include(g => g.WorkingHoursList)
            .Include(g => g.Activities)
            .ToListAsync();
    }

    // Gün ve aktiviteye göre filtrelenmiş spor salonu listesini döndürür.
    public async Task<IEnumerable<GymCenter>> GetFilteredAsync(int? dayOfWeek, int? activityId)
    {
        var query = _dbSet
            .Include(g => g.WorkingHoursList)
            .Include(g => g.Activities)
            .AsQueryable();

        // Aktiviteye göre filtreleme (SQL'de yapılabilir)
        if (activityId.HasValue)
        {
            query = query.Where(g => g.Activities.Any(a => a.Id == activityId.Value));
        }

        // Önce veritabanından çek
        var gymCenters = await query.ToListAsync();

        // Çalışma gününe göre filtreleme (memory'de yapılmalı çünkü JSON parse gerekiyor)
        if (dayOfWeek.HasValue)
        {
            gymCenters = gymCenters.Where(g =>
            {
                if (string.IsNullOrEmpty(g.WorkingHoursJson) || g.WorkingHoursJson == "[]")
                    return false;

                try
                {
                    var workingHoursData = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(g.WorkingHoursJson);
                    if (workingHoursData == null || !workingHoursData.Any())
                        return false;

                    return workingHoursData.Any(item =>
                    {
                        if (item.TryGetProperty("Day", out var dayElement))
                        {
                            return dayElement.GetInt32() == dayOfWeek.Value;
                        }
                        return false;
                    });
                }
                catch
                {
                    return false;
                }
            }).ToList();
        }

        return gymCenters;
    }
}

