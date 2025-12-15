using Microsoft.AspNetCore.Mvc;
using GymApp.Entities;
using GymApp.Services;

namespace GymApp.Controllers;

// Aktiviteleri listeleyen ve detaylarını gösteren controller.
public class ActivityController : Controller
{
    private readonly IActivityService _activityService;

    // Constructor - IActivityService bağımlılığını alır.
    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    // GET: /Activity - İsteğe göre tüm aktiviteleri veya bir salonun aktivitelerini listeler.
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

    // GET: /Activity/Details/{id} - Tek bir aktivitenin detaylarını gösterir.
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

