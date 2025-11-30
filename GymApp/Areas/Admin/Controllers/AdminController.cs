using Microsoft.AspNetCore.Mvc;
using GymApp.Entities;
using GymApp.Services;
using GymApp.Areas.Admin.ViewModels;
using System.Linq;

namespace GymApp.Areas.Admin.Controllers;

[Area("Admin")]
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

    public AdminController(
        IMemberService memberService,
        ITrainerService trainerService,
        IActivityService activityService,
        IGymCenterService gymCenterService,
        IAppointmentService appointmentService)
    {
        _memberService = memberService;
        _trainerService = trainerService;
        _activityService = activityService;
        _gymCenterService = gymCenterService;
        _appointmentService = appointmentService;
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
        ViewBag.GymCentersJson = System.Text.Json.JsonSerializer.Serialize(gymCentersWithHours);
        
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

        // Seçilen aktiviteleri TrainerActivities olarak ekle
        if (Request.Form.ContainsKey("SelectedActivityIds"))
        {
            var selectedActivityIds = Request.Form["SelectedActivityIds"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out int parsedId) ? parsedId : 0)
                .Where(id => id > 0)
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

        if (ModelState.IsValid)
        {
            // WorkingHoursJson zaten trainer içinde, direkt kaydet
            if (string.IsNullOrEmpty(trainer.WorkingHoursJson))
            {
                trainer.WorkingHoursJson = "[]";
            }
            
            await _trainerService.CreateTrainerAsync(trainer);
            return RedirectToAction("Trainers");
        }

        ViewBag.GymCenters = await _gymCenterService.GetAllGymCentersAsync();
        ViewBag.Activities = await _activityService.GetAllActivitiesAsync();
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

        // Seçilen aktiviteleri TrainerActivities olarak ekle
        if (Request.Form.ContainsKey("SelectedActivityIds"))
        {
            var selectedActivityIds = Request.Form["SelectedActivityIds"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out int parsedId) ? parsedId : 0)
                .Where(id => id > 0)
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

        if (ModelState.IsValid)
        {
            try
            {
                // WorkingHoursJson zaten trainer içinde, boşsa default değer ata
                if (string.IsNullOrEmpty(trainer.WorkingHoursJson))
                {
                    trainer.WorkingHoursJson = "[]";
                }
                
                await _trainerService.UpdateTrainerAsync(trainer);
                return RedirectToAction("Trainers");
            }
            catch
            {
                if (await _trainerService.GetTrainerByIdAsync(id) == null)
                {
                    return NotFound();
                }
                throw;
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

        // ÖNCE: ModelState'den tüm GymCenterId hatalarını temizle (Required attribute hatası dahil)
        // Bu, Required attribute'un int için 0 değerini "boş" olarak kabul etmesi sorununu çözer
        if (ModelState.ContainsKey("GymCenterId"))
        {
            ModelState["GymCenterId"]!.Errors.Clear();
        }
        if (ModelState.ContainsKey("GymCenter"))
        {
            ModelState["GymCenter"]!.Errors.Clear();
        }

        // DEBUG: Form verilerini logla
        System.Diagnostics.Debug.WriteLine("=== CreateActivity POST ===");
        System.Diagnostics.Debug.WriteLine($"GymCenterId (model - initial): {activity.GymCenterId}");
        System.Diagnostics.Debug.WriteLine($"Form Keys: {string.Join(", ", Request.Form.Keys)}");
        
        // GymCenterId'yi form'dan manuel olarak al ve parse et
        // Model binding başarısız olmuş olabilir, bu yüzden form'dan direkt alıyoruz
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
            // ModelState'den tekrar temizle (güvenlik için)
            if (ModelState.ContainsKey("GymCenterId"))
            {
                ModelState["GymCenterId"]!.Errors.Clear();
            }
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
                await _activityService.CreateActivityAsync(activity);
                return RedirectToAction("Activities");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Aktivite kaydedilirken hata oluştu: {ex.Message}");
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
        await _appointmentService.UpdateAppointmentStatusAsync(id, (AppointmentStatus)status);

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

    // Messages
    public IActionResult Messages()
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("messages");
        var model = new AdminMessagesViewModel
        {
            Inbox = GetMockMessages(),
            Reply = new AdminMessageReply()
        };

        if (TempData["MessageSuccess"] is string success)
        {
            ViewData["MessageSuccess"] = success;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReplyMessage(AdminMessageReply reply)
    {
        if (!IsAdmin())
        {
            return RedirectToLogin();
        }

        SetActiveMenu("messages");

        if (!ModelState.IsValid)
        {
            var model = new AdminMessagesViewModel
            {
                Inbox = GetMockMessages(),
                Reply = reply
            };
            return View("Messages", model);
        }

        TempData["MessageSuccess"] = $"{reply.ToEmail} adresine yanıt gönderildi.";
        return RedirectToAction("Messages");
    }
}

