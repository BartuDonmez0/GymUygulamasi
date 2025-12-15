using GymApp.Entities;
using GymApp.Repositories;
using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

// Aktivite (ders/hizmet) ile ilgili iş kurallarını yöneten servis.
public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly GymAppDbContext _context;

    // Constructor - repository ve DbContext bağımlılıklarını alır.
    public ActivityService(IActivityRepository activityRepository, GymAppDbContext context)
    {
        _activityRepository = activityRepository;
        _context = context;
    }

    // Tüm aktiviteleri spor salonu bilgisi ile birlikte döndürür.
    public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
    {
        return await _activityRepository.GetWithGymCenterAsync();
    }

    // Belirli bir spor salonuna ait aktiviteleri döndürür.
    public async Task<IEnumerable<Activity>> GetActivitiesByGymCenterIdAsync(int gymCenterId)
    {
        return await _activityRepository.GetByGymCenterIdAsync(gymCenterId);
    }

    // Id'ye göre tek bir aktiviteyi, spor salonu bilgisi ile birlikte döndürür.
    public async Task<Activity?> GetActivityByIdAsync(int id)
    {
        return await _activityRepository.GetWithGymCenterAsync(id);
    }

    // Yeni aktivite kaydı oluşturur, temel doğrulamaları yapar.
    public async Task<Activity> CreateActivityAsync(Activity activity)
    {
        // GymCenterId'nin geçerli olup olmadığını kontrol et
        var gymCenterExists = await _context.Set<GymCenter>()
            .AnyAsync(gc => gc.Id == activity.GymCenterId);
        
        if (!gymCenterExists)
        {
            throw new InvalidOperationException($"Geçersiz GymCenterId: {activity.GymCenterId}");
        }
        
        // Navigation property'leri temizle
        activity.GymCenter = null!;
        activity.TrainerActivities = new List<TrainerActivity>();
        activity.Appointments = new List<Appointment>();
        
        // ImageUrl boşsa boş string olarak ayarla
        if (string.IsNullOrEmpty(activity.ImageUrl))
        {
            activity.ImageUrl = string.Empty;
        }
        
        // Description boşsa boş string olarak ayarla
        if (string.IsNullOrEmpty(activity.Description))
        {
            activity.Description = string.Empty;
        }
        
        return await _activityRepository.AddAsync(activity);
    }

    // Var olan bir aktivite kaydını günceller.
    public async Task<Activity> UpdateActivityAsync(Activity activity)
    {
        // Mevcut activity'yi veritabanından çek (tracking için)
        var existingActivity = await _context.Set<Activity>()
            .FirstOrDefaultAsync(a => a.Id == activity.Id);
        
        if (existingActivity == null)
        {
            throw new InvalidOperationException($"Activity with Id {activity.Id} not found.");
        }
        
        // GymCenterId'nin geçerli olup olmadığını kontrol et
        var gymCenterExists = await _context.Set<GymCenter>()
            .AnyAsync(gc => gc.Id == activity.GymCenterId);
        
        if (!gymCenterExists)
        {
            throw new InvalidOperationException($"Geçersiz GymCenterId: {activity.GymCenterId}");
        }
        
        // Property'leri güncelle (navigation property'leri hariç)
        existingActivity.GymCenterId = activity.GymCenterId;
        existingActivity.Name = activity.Name;
        existingActivity.Description = string.IsNullOrEmpty(activity.Description) ? string.Empty : activity.Description;
        existingActivity.Type = activity.Type;
        existingActivity.Duration = activity.Duration;
        existingActivity.Price = activity.Price;
        existingActivity.ImageUrl = string.IsNullOrEmpty(activity.ImageUrl) ? string.Empty : activity.ImageUrl;
        
        // Değişiklikleri kaydet
        await _context.SaveChangesAsync();
        
        return existingActivity;
    }

    // Id'ye göre aktivite kaydını siler (varsa).
    public async Task DeleteActivityAsync(int id)
    {
        var activity = await _activityRepository.GetByIdAsync(id);
        if (activity != null)
        {
            await _activityRepository.DeleteAsync(activity);
        }
    }
}

