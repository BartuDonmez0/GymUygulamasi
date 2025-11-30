using Microsoft.AspNetCore.Mvc;
using GymApp.Services;
using GymApp.Entities;

namespace GymApp.Controllers;

/// <summary>
/// Trainer Controller - Antrenör listeleme ve filtreleme işlemlerini yönetir
/// Read işlemi: Tüm antrenörleri listeleme ve filtreleme
/// LINQ sorguları ile filtreleme: Aktivite ve çalışma gününe göre filtreleme yapar
/// </summary>
public class TrainerController : Controller
{
    private readonly ITrainerService _trainerService;
    private readonly IGymCenterService _gymCenterService;
    private readonly IActivityService _activityService;

    /// <summary>
    /// Constructor - Dependency injection ile servisleri alır
    /// </summary>
    public TrainerController(ITrainerService trainerService, IGymCenterService gymCenterService, IActivityService activityService)
    {
        _trainerService = trainerService;
        _gymCenterService = gymCenterService;
        _activityService = activityService;
    }

    /// <summary>
    /// Index - Antrenörleri listeler ve filtreler
    /// LINQ sorguları ile filtreleme: Aktivite ID'sine ve çalışma gününe göre filtreleme
    /// Sıralama: Müsaitlik günü sayısına ve uzmanlık alanına göre sıralama
    /// </summary>
    /// <param name="activityId">Aktivite ID'si (opsiyonel)</param>
    /// <param name="dayOfWeek">Haftanın günü (0=Pazar, 1=Pazartesi, ...) (opsiyonel)</param>
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

