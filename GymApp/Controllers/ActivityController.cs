using Microsoft.AspNetCore.Mvc;
using GymApp.Entities;
using GymApp.Services;

namespace GymApp.Controllers;

/// <summary>
/// Activity Controller - Aktivite listeleme işlemlerini yönetir
/// Read işlemi: Tüm aktiviteleri veya belirli bir spor salonuna ait aktiviteleri listeler
/// LINQ sorguları ile filtreleme: Spor salonu ID'sine göre filtreleme yapar
/// </summary>
public class ActivityController : Controller
{
    private readonly IActivityService _activityService;

    /// <summary>
    /// Constructor - Dependency injection ile servisleri alır
    /// </summary>
    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    /// <summary>
    /// Index - Aktiviteleri listeler
    /// Read işlemi: Tüm aktiviteleri veya belirli bir spor salonuna ait aktiviteleri getirir
    /// LINQ sorgusu ile filtreleme: gymCenterId parametresine göre filtreleme
    /// </summary>
    /// <param name="gymCenterId">Spor salonu ID'si (opsiyonel - tüm aktiviteler için null)</param>
    public async Task<IActionResult> Index(int? gymCenterId)
    {
        IEnumerable<Activity> activities;
        
        if (gymCenterId.HasValue)
        {
            activities = await _activityService.GetActivitiesByGymCenterIdAsync(gymCenterId.Value);
            ViewData["GymCenterId"] = gymCenterId.Value;
        }
        else
        {
            activities = await _activityService.GetAllActivitiesAsync();
        }

        return View(activities);
    }

    /// <summary>
    /// Details - Aktivite detay sayfasını gösterir
    /// Read işlemi: Belirli bir aktivitenin detaylarını getirir
    /// </summary>
    /// <param name="id">Aktivite ID'si</param>
    public async Task<IActionResult> Details(int id)
    {
        var activity = await _activityService.GetActivityByIdAsync(id);
        if (activity == null)
        {
            return NotFound();
        }
        return View(activity);
    }
}

