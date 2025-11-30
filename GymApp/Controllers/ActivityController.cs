using Microsoft.AspNetCore.Mvc;
using GymApp.Entities;
using GymApp.Services;

namespace GymApp.Controllers;

public class ActivityController : Controller
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

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

