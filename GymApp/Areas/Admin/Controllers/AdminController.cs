using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymApp.Entities;
using GymApp.Services;
using GymApp.Areas.Admin.ViewModels;
using System.Linq;

namespace GymApp.Areas.Admin.Controllers;

/// <summary>
/// Admin Controller - Admin paneli için CRUD işlemlerini yönetir
/// CRUD işlemleri: Spor salonu, antrenör, aktivite ve randevu yönetimi
/// Authorization: Sadece Admin rolü erişebilir
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin")] // Rol bazlı yetkilendirme: Sadece Admin rolü erişebilir
public class AdminController : Controller
{
    private readonly IMemberService _memberService;
    private readonly ITrainerService _trainerService;
    private readonly IActivityService _activityService;
    private readonly IGymCenterService _gymCenterService;
    private readonly IAppointmentService _appointmentService;

    private static readonly List<AdminMessageItemViewModel> MockMessages =
    [
        new()
        {
            Id = 1,
            SenderName = "Bartu Erdem",
            SenderEmail = "bartu@example.com",
            Subject = "Üyelik Yardımı",
            Body = "Hesabımdaki abonelik planını nasıl güncelleyebilirim?",
            ReceivedAt = DateTime.UtcNow.AddHours(-3),
            IsRead = false
        },
        new()
        {
            Id = 2,
            SenderName = "Selin Korkmaz",
            SenderEmail = "selin@example.com",
            Subject = "Randevu Talebi",
            Body = "Perşembe günü saat 18:00 için antrenör müsaitliği var mı?",
            ReceivedAt = DateTime.UtcNow.AddHours(-5),
            IsRead = true
        },
        new()
        {
            Id = 3,
            SenderName = "Mert Yılmaz",
            SenderEmail = "mert@example.com",
            Subject = "Salon Önerisi",
            Body = "Ankara tarafında yeni salon açmayı düşünüyor musunuz?",
            ReceivedAt = DateTime.UtcNow.AddDays(-1),
            IsRead = true
        }
    ];

    private readonly IChatMessageService _chatMessageService;

    public AdminController(
        IMemberService memberService,
        ITrainerService trainerService,
        IActivityService activityService,
        IGymCenterService gymCenterService,
        IAppointmentService appointmentService,
        IChatMessageService chatMessageService)
    {
        _memberService = memberService;
        _trainerService = trainerService;
        _activityService = activityService;
        _gymCenterService = gymCenterService;
        _appointmentService = appointmentService;
        _chatMessageService = chatMessageService;
    }

    private bool IsAdmin()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        var userEmail = HttpContext.Session.GetString("UserEmail");
        return isAdmin == "true" && userEmail == "G231210561@sakarya.edu.tr";
    }

    private IActionResult RedirectToLogin() =>
        RedirectToAction("Login", "Account", new { area = "" });

    private void SetActiveMenu(string key) => ViewData["ActiveMenu"] = key;

    private static List<AdminMessageItemViewModel> GetMockMessages() =>
        MockMessages
            .OrderByDescending(x => x.ReceivedAt)
            .ToList();

    public IActionResult Index()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("dashboard");
        return View();
    }

    // Members Management
    public async Task<IActionResult> Members()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("members");
        var members = await _memberService.GetAllMembersAsync();
        return View(members);
    }

    // Trainers Management
    public async Task<IActionResult> Trainers()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("trainers");
        var trainers = await _trainerService.GetAllTrainersAsync();
        return View(trainers);
    }

    [HttpGet]
    public async Task<IActionResult> CreateTrainer()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("trainers");
        var gymCenters = await _gymCenterService.GetAllGymCentersAsync();
        ViewBag.GymCenters = gymCenters;
        ViewBag.Activities = await _activityService.GetAllActivitiesAsync();
        
        // Spor salonlarının çalışma saatlerini JSON olarak gönder
        var gymCentersWithHours = gymCenters.Select(gc => new
        {
            Id = gc.Id,
            Name = gc.Name,
            WorkingHoursJson = gc.WorkingHoursJson ?? "[]"
        }).ToList();
        
        // JSON serialization - PropertyNamingPolicy kullanmadan direkt serialize et
        ViewBag.GymCentersJson = System.Text.Json.JsonSerializer.Serialize(gymCentersWithHours);
        
        // Debug için
        System.Diagnostics.Debug.WriteLine($"CreateTrainer - GymCenters count: {gymCenters.Count()}, JSON: {ViewBag.GymCentersJson}");
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrainer(Trainer trainer)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("trainers");

        // Şifre alanını ModelState'den temizle (view'da yok)
        ModelState.Remove("Password");
        if (string.IsNullOrEmpty(trainer.Password))
        {
            trainer.Password = "default123"; // Varsayılan şifre (gerekirse değiştirilebilir)
        }

        // Specialization alanını ModelState'den temizle (view'da yok, aktivitelerden oluşturulacak)
        ModelState.Remove("Specialization");
        // Specialization'ı seçilen aktivitelerden oluştur
        if (Request.Form.ContainsKey("SelectedActivityIds"))
        {
            var selectedActivityIds = Request.Form["SelectedActivityIds"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(id => int.TryParse(id, out _))
                .ToList();
            
            if (selectedActivityIds.Any())
            {
                var activityNames = new List<string>();
                var activities = await _activityService.GetAllActivitiesAsync();
                foreach (var activityIdStr in selectedActivityIds)
                {
                    if (int.TryParse(activityIdStr, out int activityId))
                    {
                        var activity = activities.FirstOrDefault(a => a.Id == activityId);
                        if (activity != null)
                        {
                            activityNames.Add(activity.Name);
                        }
                    }
                }
                trainer.Specialization = string.Join(", ", activityNames);
            }
            else
            {
                trainer.Specialization = "Belirtilmemiş";
            }
        }
        else
        {
            trainer.Specialization = "Belirtilmemiş";
        }

        // WorkingHoursJson'u ModelState'den temizle (form'dan manuel alacağız)
        ModelState.Remove("WorkingHoursJson");
        
        // GymCenterId'yi form'dan manuel olarak al
        if (Request.Form.ContainsKey("GymCenterId"))
        {
            var gymCenterIdValue = Request.Form["GymCenterId"].ToString();
            if (!string.IsNullOrEmpty(gymCenterIdValue) && int.TryParse(gymCenterIdValue, out int parsedGymCenterId) && parsedGymCenterId > 0)
            {
                trainer.GymCenterId = parsedGymCenterId;
                ModelState.Remove("GymCenterId");
            }
        }

        // Seçilen aktiviteleri TrainerActivities olarak ekle
        if (Request.Form.ContainsKey("SelectedActivityIds"))
        {
            var selectedActivityIds = Request.Form["SelectedActivityIds"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out int parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .Distinct() // Duplicate'leri temizle
                .ToList();
            
            trainer.TrainerActivities = selectedActivityIds.Select(activityId => new TrainerActivity
            {
                ActivityId = activityId
            }).ToList();
        }
        else
        {
            trainer.TrainerActivities = new List<TrainerActivity>();
        }

        // WorkingHoursJson'u form'dan manuel olarak al
        if (Request.Form.ContainsKey("WorkingHoursJson"))
        {
            var workingHoursJson = Request.Form["WorkingHoursJson"].ToString();
            if (!string.IsNullOrEmpty(workingHoursJson))
            {
                trainer.WorkingHoursJson = workingHoursJson;
            }
        }

        // ModelState hatalarını logla ve temizle
        var modelStateErrors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
            .ToList();
        
        if (modelStateErrors.Any())
        {
            System.Diagnostics.Debug.WriteLine("=== ModelState Errors (Before Cleanup) ===");
            foreach (var error in modelStateErrors)
            {
                System.Diagnostics.Debug.WriteLine($"Field: {error.Field}, Errors: {string.Join(", ", error.Errors ?? new List<string>())}");
            }
        }

        // Kritik olmayan navigation property hatalarını temizle
        var keysToRemove = ModelState.Keys.Where(k => 
            k.Contains("GymCenter") || 
            k.Contains("TrainerActivities") || 
            k.Contains("Appointments") || 
            k.Contains("WorkingHours")).ToList();
        
        foreach (var key in keysToRemove)
        {
            ModelState.Remove(key);
        }

        // ModelState'i tekrar kontrol et - sadece kritik hataları kontrol et
        var criticalErrors = ModelState
            .Where(x => x.Value?.Errors.Count > 0 && 
                   (x.Key == "FirstName" || x.Key == "LastName" || x.Key == "Email" || 
                    x.Key == "Phone" || x.Key == "GymCenterId"))
            .ToList();
        
        if (!criticalErrors.Any() && !string.IsNullOrEmpty(trainer.FirstName) && 
            !string.IsNullOrEmpty(trainer.LastName) && !string.IsNullOrEmpty(trainer.Email) && 
            !string.IsNullOrEmpty(trainer.Phone) && trainer.GymCenterId > 0)
        {
            // WorkingHoursJson boşsa default değer ata
            if (string.IsNullOrEmpty(trainer.WorkingHoursJson))
            {
                trainer.WorkingHoursJson = "[]";
            }
            
            // ProfilePhotoUrl boşsa boş string olarak ayarla
            if (string.IsNullOrEmpty(trainer.ProfilePhotoUrl))
            {
                trainer.ProfilePhotoUrl = string.Empty;
            }
            
            try
            {
                await _trainerService.CreateTrainerAsync(trainer);
                return RedirectToAction("Trainers");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Trainer oluşturma hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Inner exception'ı da logla
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Antrenör oluşturulurken hata oluştu: {errorMessage}");
            }
        }
        else
        {
            // Kritik hataları logla
            System.Diagnostics.Debug.WriteLine("=== Kritik Hatalar ===");
            foreach (var error in criticalErrors)
            {
                System.Diagnostics.Debug.WriteLine($"Field: {error.Key}, Errors: {string.Join(", ", error.Value?.Errors.Select(e => e.ErrorMessage) ?? new List<string>())}");
            }
            System.Diagnostics.Debug.WriteLine($"FirstName: {trainer.FirstName}, LastName: {trainer.LastName}, Email: {trainer.Email}, Phone: {trainer.Phone}, GymCenterId: {trainer.GymCenterId}");
        }

        // Hata varsa ViewBag'leri tekrar set et
        ViewBag.GymCenters = await _gymCenterService.GetAllGymCentersAsync();
        ViewBag.Activities = await _activityService.GetAllActivitiesAsync();
        
        // Spor salonlarının çalışma saatlerini JSON olarak gönder
        var gymCenters = await _gymCenterService.GetAllGymCentersAsync();
        var gymCentersWithHours = gymCenters.Select(gc => new
        {
            Id = gc.Id,
            Name = gc.Name,
            WorkingHoursJson = gc.WorkingHoursJson ?? "[]"
        }).ToList();
        ViewBag.GymCentersJson = System.Text.Json.JsonSerializer.Serialize(gymCentersWithHours);
        
        return View(trainer);
    }

    [HttpGet]
    public async Task<IActionResult> EditTrainer(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        var trainer = await _trainerService.GetTrainerByIdAsync(id);
        if (trainer == null)
        {
            return NotFound();
        }

        SetActiveMenu("trainers");
        var gymCenters = await _gymCenterService.GetAllGymCentersAsync();
        ViewBag.GymCenters = gymCenters;
        ViewBag.Activities = await _activityService.GetAllActivitiesAsync();
        
        // Spor salonlarının çalışma saatlerini JSON olarak gönder
        var gymCentersWithHours = gymCenters.Select(gc => new
        {
            Id = gc.Id,
            Name = gc.Name,
            WorkingHoursJson = gc.WorkingHoursJson ?? "[]"
        }).ToList();
        ViewBag.GymCentersJson = System.Text.Json.JsonSerializer.Serialize(gymCentersWithHours);
        
        // WorkingHoursJson boşsa default değer ata
        if (string.IsNullOrEmpty(trainer.WorkingHoursJson))
        {
            trainer.WorkingHoursJson = "[]";
        }
        
        return View(trainer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTrainer(int id, Trainer trainer)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        if (id != trainer.Id)
        {
            return NotFound();
        }

        SetActiveMenu("trainers");

        // Şifre alanını ModelState'den temizle (view'da yok, güncellemede şifre değiştirilmez)
        ModelState.Remove("Password");
        // Mevcut şifreyi koru (güncellemede şifre değiştirilmez)
        var existingTrainer = await _trainerService.GetTrainerByIdAsync(id);
        if (existingTrainer != null && string.IsNullOrEmpty(trainer.Password))
        {
            trainer.Password = existingTrainer.Password;
        }

        // WorkingHoursJson'u ModelState'den temizle (form'dan manuel alacağız)
        ModelState.Remove("WorkingHoursJson");
        
        // GymCenterId'yi form'dan manuel olarak al
        if (Request.Form.ContainsKey("GymCenterId"))
        {
            var gymCenterIdValue = Request.Form["GymCenterId"].ToString();
            if (!string.IsNullOrEmpty(gymCenterIdValue) && int.TryParse(gymCenterIdValue, out int parsedGymCenterId) && parsedGymCenterId > 0)
            {
                trainer.GymCenterId = parsedGymCenterId;
                ModelState.Remove("GymCenterId");
            }
        }

        // Seçilen aktiviteleri TrainerActivities olarak ekle
        if (Request.Form.ContainsKey("SelectedActivityIds"))
        {
            var selectedActivityIds = Request.Form["SelectedActivityIds"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out int parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .Distinct() // Duplicate'leri temizle
                .ToList();
            
            trainer.TrainerActivities = selectedActivityIds.Select(activityId => new TrainerActivity
            {
                ActivityId = activityId
            }).ToList();
        }
        else
        {
            trainer.TrainerActivities = new List<TrainerActivity>();
        }

        // WorkingHoursJson'u form'dan manuel olarak al
        if (Request.Form.ContainsKey("WorkingHoursJson"))
        {
            var workingHoursJson = Request.Form["WorkingHoursJson"].ToString();
            if (!string.IsNullOrEmpty(workingHoursJson))
            {
                trainer.WorkingHoursJson = workingHoursJson;
            }
        }

        // ModelState'i kontrol et - sadece kritik hataları kontrol et
        var criticalErrors = ModelState
            .Where(x => x.Value?.Errors.Count > 0 && 
                   (x.Key == "FirstName" || x.Key == "LastName" || x.Key == "Email" || 
                    x.Key == "Phone" || x.Key == "GymCenterId"))
            .ToList();
        
        if (!criticalErrors.Any() && !string.IsNullOrEmpty(trainer.FirstName) && 
            !string.IsNullOrEmpty(trainer.LastName) && !string.IsNullOrEmpty(trainer.Email) && 
            !string.IsNullOrEmpty(trainer.Phone) && trainer.GymCenterId > 0)
        {
            try
            {
                // WorkingHoursJson boşsa default değer ata
                if (string.IsNullOrEmpty(trainer.WorkingHoursJson))
                {
                    trainer.WorkingHoursJson = "[]";
                }
                
                // ProfilePhotoUrl boşsa boş string olarak ayarla
                if (string.IsNullOrEmpty(trainer.ProfilePhotoUrl))
                {
                    trainer.ProfilePhotoUrl = string.Empty;
                }
                
                await _trainerService.UpdateTrainerAsync(trainer);
                return RedirectToAction("Trainers");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Trainer güncelleme hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Antrenör güncellenirken hata oluştu: {ex.Message}");
                
                if (await _trainerService.GetTrainerByIdAsync(id) == null)
                {
                    return NotFound();
                }
            }
        }

        ViewBag.GymCenters = await _gymCenterService.GetAllGymCentersAsync();
        ViewBag.Activities = await _activityService.GetAllActivitiesAsync();
        return View(trainer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrainer(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("trainers");
        await _trainerService.DeleteTrainerAsync(id);

        return RedirectToAction("Trainers");
    }

    // Activities Management
    public async Task<IActionResult> Activities()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("activities");
        var activities = await _activityService.GetAllActivitiesAsync();
        return View(activities);
    }

    [HttpGet]
    public async Task<IActionResult> CreateActivity()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("activities");
        ViewBag.GymCenters = await _gymCenterService.GetAllGymCentersAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateActivity(Activity activity)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("activities");

        // ÖNCE: ModelState'den tüm GymCenterId ve navigation property hatalarını temizle
        ModelState.Remove("GymCenterId");
        ModelState.Remove("GymCenter");
        ModelState.Remove("TrainerActivities");
        ModelState.Remove("Appointments");

        // DEBUG: Form verilerini logla
        System.Diagnostics.Debug.WriteLine("=== CreateActivity POST ===");
        System.Diagnostics.Debug.WriteLine($"GymCenterId (model - initial): {activity.GymCenterId}");
        System.Diagnostics.Debug.WriteLine($"Form Keys: {string.Join(", ", Request.Form.Keys)}");
        
        // GymCenterId'yi form'dan manuel olarak al ve parse et
        bool gymCenterIdFound = false;
        if (Request.Form.ContainsKey("GymCenterId"))
        {
            var gymCenterIdValue = Request.Form["GymCenterId"].ToString();
            System.Diagnostics.Debug.WriteLine($"Form GymCenterId value: '{gymCenterIdValue}'");
            
            if (!string.IsNullOrEmpty(gymCenterIdValue) && int.TryParse(gymCenterIdValue, out int parsedId) && parsedId > 0)
            {
                activity.GymCenterId = parsedId;
                gymCenterIdFound = true;
                System.Diagnostics.Debug.WriteLine($"Successfully parsed and set GymCenterId: {parsedId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to parse GymCenterId: '{gymCenterIdValue}'");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("GymCenterId not found in Request.Form");
        }

        // GymCenterId kontrolü - eğer hala 0 ise veya bulunamadıysa hata ekle
        if (activity.GymCenterId == 0 || !gymCenterIdFound)
        {
            System.Diagnostics.Debug.WriteLine($"GymCenterId validation failed: {activity.GymCenterId}, found: {gymCenterIdFound}");
            ModelState.AddModelError("GymCenterId", "Spor salonu seçimi zorunludur.");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"GymCenterId is valid: {activity.GymCenterId}");
            ModelState.Remove("GymCenterId"); // ModelState'den temizle
        }

        // ImageUrl boşsa boş string olarak ayarla
        if (string.IsNullOrEmpty(activity.ImageUrl))
        {
            activity.ImageUrl = string.Empty;
            // ModelState'den ImageUrl validation hatasını temizle
            ModelState.Remove("ImageUrl");
        }

        // Description boşsa boş string olarak ayarla
        if (string.IsNullOrEmpty(activity.Description))
        {
            activity.Description = string.Empty;
        }

        // Type enum binding kontrolü - form'dan manuel al
        if (Request.Form.ContainsKey("Type"))
        {
            var typeValue = Request.Form["Type"].ToString();
            System.Diagnostics.Debug.WriteLine($"Form Type value: '{typeValue}'");
            
            if (!string.IsNullOrEmpty(typeValue) && int.TryParse(typeValue, out int parsedType) && parsedType > 0)
            {
                if (Enum.IsDefined(typeof(ActivityType), parsedType))
                {
                    activity.Type = (ActivityType)parsedType;
                    System.Diagnostics.Debug.WriteLine($"Successfully parsed and set Type: {parsedType}");
                    ModelState.Remove("Type"); // ModelState'den temizle
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid Type value: {parsedType}");
                    ModelState.AddModelError("Type", "Geçersiz aktivite tipi seçildi.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to parse Type: '{typeValue}'");
                ModelState.AddModelError("Type", "Aktivite tipi seçimi zorunludur.");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Type not found in Request.Form");
            if (activity.Type == 0)
            {
                ModelState.AddModelError("Type", "Aktivite tipi seçimi zorunludur.");
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _activityService.CreateActivityAsync(activity);
                return RedirectToAction("Activities");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Activity oluşturma hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Inner exception'ı da logla
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Aktivite kaydedilirken hata oluştu: {errorMessage}");
            }
        }
        else
        {
            // ModelState hatalarını logla ve kullanıcıya göster
            var errorMessages = new List<string>();
            foreach (var key in ModelState.Keys)
            {
                if (!ModelState.TryGetValue(key, out var entry) || entry == null) continue;
                var errors = entry.Errors;
                foreach (var error in errors)
                {
                    var errorMsg = $"{key}: {error.ErrorMessage}";
                    errorMessages.Add(errorMsg);
                    System.Diagnostics.Debug.WriteLine($"ModelState Error - {errorMsg}");
                }
            }
            if (errorMessages.Any())
            {
                ModelState.AddModelError("", "Lütfen tüm zorunlu alanları doldurun: " + string.Join(", ", errorMessages));
            }
        }

        ViewBag.GymCenters = await _gymCenterService.GetAllGymCentersAsync();
        return View(activity);
    }

    [HttpGet]
    public async Task<IActionResult> EditActivity(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        var activity = await _activityService.GetActivityByIdAsync(id);
        if (activity == null)
        {
            return NotFound();
        }

        SetActiveMenu("activities");
        ViewBag.GymCenters = await _gymCenterService.GetAllGymCentersAsync();
        return View(activity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditActivity(int id, Activity activity)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        if (id != activity.Id)
        {
            return NotFound();
        }

        SetActiveMenu("activities");

        // ImageUrl boşsa boş string olarak ayarla
        if (string.IsNullOrEmpty(activity.ImageUrl))
        {
            activity.ImageUrl = string.Empty;
            // ModelState'den ImageUrl validation hatasını temizle
            ModelState.Remove("ImageUrl");
        }

        // Description boşsa boş string olarak ayarla
        if (string.IsNullOrEmpty(activity.Description))
        {
            activity.Description = string.Empty;
        }

        // Type enum binding kontrolü - eğer 0 ise (default enum değeri) hata ekle
        if (activity.Type == 0)
        {
            ModelState.AddModelError("Type", "Aktivite tipi seçimi zorunludur.");
        }
        else
        {
            // Type enum değeri geçerli mi kontrol et
            if (!Enum.IsDefined(typeof(ActivityType), activity.Type))
            {
                ModelState.AddModelError("Type", "Geçersiz aktivite tipi seçildi.");
            }
            else
            {
                // Geçerli bir enum değeri ise ModelState'den Type hatasını temizle
                if (ModelState.ContainsKey("Type"))
                {
                    ModelState.Remove("Type");
                }
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _activityService.UpdateActivityAsync(activity);
                return RedirectToAction("Activities");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Aktivite güncellenirken hata oluştu: {ex.Message}");
            }
        }
        else
        {
            // ModelState hatalarını logla ve kullanıcıya göster
            var errorMessages = new List<string>();
            foreach (var key in ModelState.Keys)
            {
                if (!ModelState.TryGetValue(key, out var entry) || entry == null) continue;
                var errors = entry.Errors;
                foreach (var error in errors)
                {
                    var errorMsg = $"{key}: {error.ErrorMessage}";
                    errorMessages.Add(errorMsg);
                    System.Diagnostics.Debug.WriteLine($"ModelState Error - {errorMsg}");
                }
            }
            if (errorMessages.Any())
            {
                ModelState.AddModelError("", "Lütfen tüm zorunlu alanları doldurun: " + string.Join(", ", errorMessages));
            }
        }

        ViewBag.GymCenters = await _gymCenterService.GetAllGymCentersAsync();
        return View(activity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("activities");
        await _activityService.DeleteActivityAsync(id);

        return RedirectToAction("Activities");
    }

    // GymCenters Management
    public async Task<IActionResult> GymCenters()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("gyms");
        var gymCenters = await _gymCenterService.GetAllGymCentersAsync();
        return View(gymCenters);
    }

    [HttpGet]
    public IActionResult CreateGymCenter()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("gyms");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGymCenter(GymCenter gymCenter)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("gyms");

        if (ModelState.IsValid)
        {
            // WorkingHoursJson zaten gymCenter içinde, boşsa default değer ata
            if (string.IsNullOrEmpty(gymCenter.WorkingHoursJson))
            {
                gymCenter.WorkingHoursJson = "[]";
            }
            
            await _gymCenterService.CreateGymCenterAsync(gymCenter);
            return RedirectToAction("GymCenters");
        }

        return View(gymCenter);
    }

    [HttpGet]
    public async Task<IActionResult> EditGymCenter(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        var gymCenter = await _gymCenterService.GetGymCenterByIdAsync(id);
        if (gymCenter == null)
        {
            return NotFound();
        }

        SetActiveMenu("gyms");
        
        // WorkingHoursJson boşsa default değer ata
        if (string.IsNullOrEmpty(gymCenter.WorkingHoursJson))
        {
            gymCenter.WorkingHoursJson = "[]";
        }
        
        return View(gymCenter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGymCenter(int id, GymCenter gymCenter)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        if (id != gymCenter.Id)
        {
            return NotFound();
        }

        SetActiveMenu("gyms");

        if (ModelState.IsValid)
        {
            try
            {
                // WorkingHoursJson zaten gymCenter içinde, boşsa default değer ata
                if (string.IsNullOrEmpty(gymCenter.WorkingHoursJson))
                {
                    gymCenter.WorkingHoursJson = "[]";
                }
                
                await _gymCenterService.UpdateGymCenterAsync(gymCenter);
                return RedirectToAction("GymCenters");
            }
            catch
            {
                if (await _gymCenterService.GetGymCenterByIdAsync(id) == null)
                {
                    return NotFound();
                }
                throw;
            }
        }

        return View(gymCenter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGymCenter(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("gyms");
        await _gymCenterService.DeleteGymCenterAsync(id);

        return RedirectToAction("GymCenters");
    }

    // Appointments Management
    public async Task<IActionResult> Appointments()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("appointments");
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, int status)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("appointments");
        
        try
        {
            await _appointmentService.UpdateAppointmentStatusAsync(id, (AppointmentStatus)status);
            TempData["Success"] = "Randevu durumu başarıyla güncellendi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Randevu durumu güncellenirken hata oluştu: {ex.Message}";
        }

        return RedirectToAction("Appointments");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("appointments");
        await _appointmentService.DeleteAppointmentAsync(id);

        return RedirectToAction("Appointments");
    }

    // AI Conversations
    public async Task<IActionResult> AIConversations()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("aiconversations");
        
        // Tüm AI görüşmelerini getir
        var chatMessages = await _chatMessageService.GetAllChatMessagesAsync();
        
        // Kullanıcıya göre grupla
        var conversations = chatMessages
            .GroupBy(cm => cm.MemberId)
            .Select(g => new
            {
                MemberId = g.Key,
                MemberName = (g.First().Member?.FirstName ?? "") + " " + (g.First().Member?.LastName ?? ""),
                MemberEmail = g.First().Member?.Email ?? "",
                MessageCount = g.Count(),
                LastMessageDate = g.Max(cm => cm.CreatedDate),
                Messages = g.OrderBy(cm => cm.CreatedDate).ToList()
            })
            .OrderByDescending(c => c.LastMessageDate)
            .ToList();

        ViewBag.Conversations = conversations;
        ViewBag.AllMessages = chatMessages.OrderByDescending(cm => cm.CreatedDate).ToList();
        
        return View();
    }
}

