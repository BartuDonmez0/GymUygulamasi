using Microsoft.AspNetCore.Mvc;
using GymApp.Services;
using GymApp.Entities;

namespace GymApp.Controllers;

// Antrenörleri listeleyen ve filtreleyen controller.
public class TrainerController : Controller
{
    private readonly ITrainerService _trainerService;
    private readonly IGymCenterService _gymCenterService;
    private readonly IActivityService _activityService;

    // Constructor - antrenör, salon ve aktivite servislerini alır.
    public TrainerController(ITrainerService trainerService, IGymCenterService gymCenterService, IActivityService activityService)
    {
        _trainerService = trainerService;
        _gymCenterService = gymCenterService;
        _activityService = activityService;
    }

    // GET: /Trainer - Aktivite ve güne göre filtrelenmiş antrenör listesini gösterir.
    public async Task<IActionResult> Index(int? activityId, int? dayOfWeek)
    {
        // Antrenörleri çek
        var trainers = await _trainerService.GetAllTrainersAsync();
        
        // Aktiviteye göre filtreleme
        if (activityId.HasValue && activityId.Value > 0)
        {
            trainers = trainers.Where(t => 
                t.TrainerActivities != null && 
                t.TrainerActivities.Any(ta => ta.ActivityId == activityId.Value));
        }
        
        // Çalışma gününe göre filtreleme
        if (dayOfWeek.HasValue)
        {
            trainers = trainers.Where(t =>
            {
                if (string.IsNullOrEmpty(t.WorkingHoursJson) || t.WorkingHoursJson == "[]")
                    return false;
                
                try
                {
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(t.WorkingHoursJson);
                    return jsonDoc.RootElement.EnumerateArray()
                        .Any(item => item.TryGetProperty("Day", out var dayElement) && 
                                    dayElement.ValueKind == System.Text.Json.JsonValueKind.Number &&
                                    dayElement.GetInt32() == dayOfWeek.Value);
                }
                catch
                {
                    return false;
                }
            });
        }
        
        // Müsaitlik günlerine ve uzmanlık alanlarına göre sırala
        var sortedTrainers = trainers
            .OrderByDescending(t => 
            {
                // Önce müsaitlik günü sayısına göre (daha fazla gün olanlar önce)
                if (!string.IsNullOrEmpty(t.WorkingHoursJson) && t.WorkingHoursJson != "[]")
                {
                    try
                    {
                        var jsonDoc = System.Text.Json.JsonDocument.Parse(t.WorkingHoursJson);
                        return jsonDoc.RootElement.EnumerateArray().Count();
                    }
                    catch
                    {
                        return 0;
                    }
                }
                return 0;
            })
            .ThenBy(t => t.Specialization) // Sonra uzmanlık alanına göre alfabetik
            .ToList();

        // Tüm aktiviteleri al (filtreleme için)
        var allActivities = await _activityService.GetAllActivitiesAsync();

        ViewBag.Activities = allActivities;
        ViewBag.SelectedActivityId = activityId;
        ViewBag.SelectedDayOfWeek = dayOfWeek;

        return View(sortedTrainers);
    }
}

