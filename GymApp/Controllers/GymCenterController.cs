using Microsoft.AspNetCore.Mvc;
using GymApp.Entities;
using GymApp.Services;

namespace GymApp.Controllers;

/// <summary>
/// GymCenter Controller - Spor salonu yönetimi için CRUD işlemlerini yönetir
/// Read işlemi: Tüm spor salonlarını listeleme ve filtreleme
/// </summary>
public class GymCenterController : Controller
{
    private readonly IGymCenterService _gymCenterService;
    private readonly IActivityService _activityService;
    private readonly ITrainerService _trainerService;

    /// <summary>
    /// Constructor - Dependency injection ile servisleri alır
    /// </summary>
    public GymCenterController(IGymCenterService gymCenterService, IActivityService activityService, ITrainerService trainerService)
    {
        _gymCenterService = gymCenterService;
        _activityService = activityService;
        _trainerService = trainerService;
    }

    /// <summary>
    /// Index - Spor salonlarını listeler
    /// LINQ sorgusu ile filtreleme: Çalışma günü ve aktiviteye göre filtreleme yapar
    /// </summary>
    /// <param name="dayOfWeek">Haftanın günü (0=Pazar, 1=Pazartesi, ...)</param>
    /// <param name="activityId">Aktivite ID'si</param>
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

    /// <summary>
    /// Details - Spor salonu detay sayfasını gösterir
    /// Read işlemi: Belirli bir spor salonunun detaylarını, aktivitelerini ve antrenörlerini getirir
    /// </summary>
    /// <param name="id">Spor salonu ID'si</param>
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

    /// <summary>
    /// Activities - Spor salonuna ait aktiviteleri gösterir
    /// Read işlemi: Belirli bir spor salonunun aktivitelerini listeler
    /// </summary>
    /// <param name="id">Spor salonu ID'si</param>
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

