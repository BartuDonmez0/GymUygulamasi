using Microsoft.AspNetCore.Mvc;
using GymApp.Entities;
using GymApp.Services;

namespace GymApp.Controllers;

// Spor salonlarını listeleyen ve detaylarını gösteren controller.
public class GymCenterController : Controller
{
    private readonly IGymCenterService _gymCenterService;
    private readonly IActivityService _activityService;
    private readonly ITrainerService _trainerService;

    // Constructor - salon, aktivite ve antrenör servislerini alır.
    public GymCenterController(IGymCenterService gymCenterService, IActivityService activityService, ITrainerService trainerService)
    {
        _gymCenterService = gymCenterService;
        _activityService = activityService;
        _trainerService = trainerService;
    }

    // GET: /GymCenter - Gün ve aktiviteye göre filtrelenmiş salon listesini gösterir.
    public async Task<IActionResult> Index(int? dayOfWeek, int? activityId)
    {
        IEnumerable<GymCenter> gymCenters;
        
        if (dayOfWeek.HasValue || activityId.HasValue)
        {
            gymCenters = await _gymCenterService.GetFilteredGymCentersAsync(dayOfWeek, activityId);
        }
        else
        {
            gymCenters = await _gymCenterService.GetAllGymCentersAsync();
        }

        ViewBag.DayOfWeek = dayOfWeek;
        ViewBag.ActivityId = activityId;
        ViewBag.Activities = await _activityService.GetAllActivitiesAsync();
        
        return View(gymCenters);
    }

    // GET: /GymCenter/Details/{id} - Salon detayları, aktiviteleri ve antrenörlerini gösterir.
    public async Task<IActionResult> Details(int id)
    {
        var gymCenter = await _gymCenterService.GetGymCenterWithDetailsAsync(id);
        if (gymCenter == null)
        {
            return NotFound();
        }
        
        // Bu spor salonuna ait aktiviteleri çek - LINQ sorgusu ile filtreleme
        var activities = await _activityService.GetActivitiesByGymCenterIdAsync(id);
        
        // Bu spor salonuna ait antrenörleri çek - LINQ sorgusu ile filtreleme
        var allTrainers = await _trainerService.GetAllTrainersAsync();
        var trainers = allTrainers.Where(t => t.GymCenterId == id).ToList();
        
        ViewBag.Activities = activities;
        ViewBag.Trainers = trainers;
        
        return View(gymCenter);
    }

    // GET: /GymCenter/Activities/{id} - İlgili salonun aktiviteleri sayfasına yönlendirir.
    public async Task<IActionResult> Activities(int id)
    {
        var gymCenter = await _gymCenterService.GetGymCenterByIdAsync(id);
        if (gymCenter == null)
        {
            return NotFound();
        }
        return RedirectToAction("Index", "Activity", new { gymCenterId = id });
    }
}

