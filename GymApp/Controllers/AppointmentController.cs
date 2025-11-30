using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymApp.Services;
using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Controllers;

/// <summary>
/// Appointment Controller - Randevu yönetimi için CRUD işlemlerini yönetir
/// Create işlemi: Yeni randevu oluşturma
/// Read işlemi: Randevu listeleme (kullanıcı kendi randevularını, admin tüm randevuları görür)
/// Authorization: Giriş yapmış kullanıcılar erişebilir, admin tüm randevuları görebilir
/// </summary>
[Authorize(Roles = "Admin,User")] // Rol bazlı yetkilendirme: Admin veya User rolleri erişebilir
public class AppointmentController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IMemberRepository _memberRepository;
    private readonly ITrainerService _trainerService;
    private readonly IActivityService _activityService;
    private readonly IGymCenterService _gymCenterService;

    /// <summary>
    /// Constructor - Dependency injection ile servisleri alır
    /// </summary>
    public AppointmentController(
        IAppointmentService appointmentService,
        IMemberRepository memberRepository,
        ITrainerService trainerService,
        IActivityService activityService,
        IGymCenterService gymCenterService)
    {
        _appointmentService = appointmentService;
        _memberRepository = memberRepository;
        _trainerService = trainerService;
        _activityService = activityService;
        _gymCenterService = gymCenterService;
    }

    /// <summary>
    /// Index - Randevuları listeler
    /// Read işlemi: Kullanıcı kendi randevularını, admin tüm randevuları görür
    /// Authorization: Giriş yapmış kullanıcılar erişebilir
    /// Rol bazlı yetkilendirme: Admin ve Üye ayrımı yapılır
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // Giriş kontrolü
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login", "Account");
        }

        // Admin kontrolü - admin ise tüm randevuları göster
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        IEnumerable<Appointment> appointments;

        if (isAdmin == "true")
        {
            appointments = await _appointmentService.GetAllAppointmentsAsync();
        }
        else
        {
            // Normal kullanıcı - sadece kendi randevularını göster
            var member = await _memberRepository.GetByEmailAsync(userEmail);
            if (member == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction("Login", "Account");
            }

            appointments = await _appointmentService.GetAppointmentsByMemberIdAsync(member.Id);
        }

        return View(appointments);
    }

    /// <summary>
    /// Create GET - Yeni randevu oluşturma formunu gösterir
    /// Create işlemi: Randevu oluşturma formu
    /// Authorization: Giriş yapmış üyeler erişebilir, admin erişemez
    /// </summary>
    /// <param name="trainerId">Antrenör ID'si (opsiyonel - önceden seçilmiş antrenör için)</param>
    [HttpGet]
    public async Task<IActionResult> Create(int? trainerId)
    {
        // Giriş kontrolü
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            TempData["Message"] = "Randevu almak için lütfen giriş yapın.";
            return RedirectToAction("Login", "Account");
        }

        // Admin kontrolü - Admin randevu oluşturamaz
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        if (isAdmin == "true")
        {
            TempData["Error"] = "Admin kullanıcıları randevu oluşturamaz.";
            return RedirectToAction("Index", "Appointment");
        }

        // Member'ı bul
        var member = await _memberRepository.GetByEmailAsync(userEmail);
        if (member == null)
        {
            TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
            return RedirectToAction("Login", "Account");
        }

        // Dropdown verilerini hazırla
        var trainers = await _trainerService.GetAllTrainersAsync();
        var activities = await _activityService.GetAllActivitiesAsync();
        var gymCenters = await _gymCenterService.GetAllGymCentersAsync();

        ViewBag.MemberId = member.Id;
        ViewBag.Trainers = trainers;
        ViewBag.Activities = activities;
        ViewBag.GymCenters = gymCenters;
        ViewBag.SelectedTrainerId = trainerId;

        return View();
    }

    /// <summary>
    /// GetTrainerData - Antrenör bilgilerini JSON formatında döndürür
    /// AJAX endpoint: Antrenör seçildiğinde aktiviteleri, spor salonunu ve çalışma saatlerini getirir
    /// </summary>
    /// <param name="trainerId">Antrenör ID'si</param>
    [HttpGet]
    public async Task<IActionResult> GetTrainerData(int trainerId)
    {
        var trainer = await _trainerService.GetTrainerByIdAsync(trainerId);
        if (trainer == null)
        {
            return Json(new { success = false, message = "Antrenör bulunamadı." });
        }

        // Antrenörün aktivitelerini al
        var activitiesList = new List<object>();
        if (trainer.TrainerActivities != null && trainer.TrainerActivities.Any())
        {
            foreach (var ta in trainer.TrainerActivities.Where(ta => ta.Activity != null))
            {
                activitiesList.Add(new
                {
                    id = ta.Activity!.Id,
                    name = ta.Activity.Name,
                    price = ta.Activity.Price
                });
            }
        }
        
        // Eğer aktivite yoksa bile success: true döndür, activities boş array olsun
        // JavaScript tarafında kontrol edilecek

        // Antrenörün spor salonu bilgisini al
        var gymCenterId = trainer.GymCenterId;
        var gymCenterName = trainer.GymCenter?.Name ?? "";

        // Antrenörün çalışma saatlerini parse et
        var workingHours = new List<object>();
        if (!string.IsNullOrEmpty(trainer.WorkingHoursJson) && trainer.WorkingHoursJson != "[]")
        {
            try
            {
                var jsonDoc = System.Text.Json.JsonDocument.Parse(trainer.WorkingHoursJson);
                foreach (var item in jsonDoc.RootElement.EnumerateArray())
                {
                    if (item.TryGetProperty("Day", out var dayElement) &&
                        dayElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        var day = dayElement.GetInt32();
                        if (item.TryGetProperty("Hours", out var hoursElement) &&
                            hoursElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            var hours = hoursElement.EnumerateArray()
                                .Select(h => h.GetString())
                                .Where(h => !string.IsNullOrEmpty(h))
                                .ToList();
                            
                            foreach (var hour in hours)
                            {
                                workingHours.Add(new { day = day, hour = hour });
                            }
                        }
                        else if (item.TryGetProperty("TimeRange", out var timeRangeElement) &&
                                 timeRangeElement.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            var timeRange = timeRangeElement.GetString();
                            workingHours.Add(new { day = day, timeRange = timeRange });
                        }
                    }
                }
            }
            catch
            {
                // JSON parse hatası
            }
        }

        return Json(new
        {
            success = true,
            gymCenterId = gymCenterId,
            gymCenterName = gymCenterName,
            activities = activitiesList,
            workingHours = workingHours
        });
    }

    /// <summary>
    /// CheckAvailability - Antrenörün belirli tarih ve saatte müsait olup olmadığını kontrol eder
    /// AJAX endpoint: Client-side validation için kullanılır
    /// </summary>
    /// <param name="trainerId">Antrenör ID'si</param>
    /// <param name="date">Randevu tarihi</param>
    /// <param name="time">Randevu saati</param>
    [HttpGet]
    public async Task<IActionResult> CheckAvailability(int trainerId, string date, string time)
    {
        if (DateTime.TryParse(date, out var appointmentDate) && TimeSpan.TryParse(time, out var appointmentTime))
        {
            var exists = await _appointmentService.ExistsAtSameTimeAsync(trainerId, appointmentDate, appointmentTime);
            return Json(new { exists = exists });
        }
        return Json(new { exists = false });
    }

    /// <summary>
    /// CheckMemberAvailability - Kullanıcının belirli tarih ve saatte onaylı randevusu olup olmadığını kontrol eder
    /// AJAX endpoint: Client-side validation için kullanılır
    /// </summary>
    /// <param name="date">Randevu tarihi</param>
    /// <param name="time">Randevu saati</param>
    [HttpGet]
    public async Task<IActionResult> CheckMemberAvailability(string date, string time)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Json(new { exists = false });
        }

        var member = await _memberRepository.GetByEmailAsync(userEmail);
        if (member == null)
        {
            return Json(new { exists = false });
        }

        if (DateTime.TryParse(date, out var appointmentDate) && TimeSpan.TryParse(time, out var appointmentTime))
        {
            var exists = await _appointmentService.ExistsAtSameTimeForMemberAsync(member.Id, appointmentDate, appointmentTime);
            return Json(new { exists = exists });
        }
        return Json(new { exists = false });
    }

    /// <summary>
    /// GetBookedTimes - Belirli bir tarihte rezerve edilmiş saatleri getirir
    /// AJAX endpoint: Randevu formunda müsait saatleri göstermek için kullanılır
    /// LINQ sorgusu ile filtreleme: Tarih ve duruma göre filtreleme yapar
    /// </summary>
    /// <param name="trainerId">Antrenör ID'si</param>
    /// <param name="date">Randevu tarihi</param>
    [HttpGet]
    public async Task<IActionResult> GetBookedTimes(int trainerId, string date)
    {
        if (DateTime.TryParse(date, out var appointmentDate))
        {
            // Antrenör için o tarihte onaylı randevuları al
            var trainerAppointments = await _appointmentService.GetAllAppointmentsAsync();
            var bookedTimes = trainerAppointments
                .Where(a => a.TrainerId == trainerId &&
                           a.AppointmentDate.Date == appointmentDate.Date &&
                           a.Status == AppointmentStatus.Approved)
                .Select(a => a.AppointmentTime.ToString(@"hh\:mm"))
                .ToList();

            // Kullanıcının o tarihte onaylı randevularını al (kendi onaylı randevusunun yanında yeni randevu oluşturamasın)
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var memberBookedTimes = new List<string>();
            if (!string.IsNullOrEmpty(userEmail))
            {
                var member = await _memberRepository.GetByEmailAsync(userEmail);
                if (member != null)
                {
                    var memberAppointments = await _appointmentService.GetAppointmentsByMemberIdAsync(member.Id);
                    memberBookedTimes = memberAppointments
                        .Where(a => a.AppointmentDate.Date == appointmentDate.Date &&
                                   a.Status == AppointmentStatus.Approved)
                        .Select(a => a.AppointmentTime.ToString(@"hh\:mm"))
                        .ToList();
                }
            }

            // Her iki listeyi birleştir (antrenör ve kullanıcı için onaylı randevular)
            var allBookedTimes = bookedTimes.Union(memberBookedTimes).Distinct().ToList();

            return Json(new { bookedTimes = allBookedTimes });
        }
        return Json(new { bookedTimes = new List<string>() });
    }

    /// <summary>
    /// Create POST - Yeni randevu oluşturur
    /// Create işlemi: Randevu kaydı oluşturma
    /// Server-side validation: ModelState.IsValid kontrolü ve özel validasyonlar
    /// Authorization: Sadece User rolü erişebilir, admin erişemez
    /// </summary>
    /// <param name="appointment">Randevu bilgileri</param>
    [HttpPost]
    [ValidateAntiForgeryToken] // CSRF koruması
    [Authorize(Roles = "User")] // Rol bazlı yetkilendirme: Sadece User rolü erişebilir (Admin erişemez)
    public async Task<IActionResult> Create(Appointment appointment)
    {
        // Giriş kontrolü
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            TempData["Message"] = "Randevu almak için lütfen giriş yapın.";
            return RedirectToAction("Login", "Account");
        }

        // Admin kontrolü - Admin randevu oluşturamaz
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        if (isAdmin == "true")
        {
            TempData["Error"] = "Admin kullanıcıları randevu oluşturamaz.";
            return RedirectToAction("Index", "Appointment");
        }

        // Member'ı bul
        var member = await _memberRepository.GetByEmailAsync(userEmail);
        if (member == null)
        {
            TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
            return RedirectToAction("Login", "Account");
        }

        // Dropdown verilerini yükle (hem hata durumunda hem de başarılı durumda kullanılacak)
        var allTrainers = await _trainerService.GetAllTrainersAsync();
        var allActivities = await _activityService.GetAllActivitiesAsync();
        var allGymCenters = await _gymCenterService.GetAllGymCentersAsync();

        // MemberId'yi set et (kontrol için gerekli)
        appointment.MemberId = member.Id;
        
        // ModelState'i temizle (MemberId manuel set edildi)
        ModelState.Remove("MemberId");
        
        // ZORUNLU ALAN KONTROLÜ
        if (appointment.TrainerId <= 0)
        {
            ModelState.AddModelError("TrainerId", "Antrenör seçimi zorunludur.");
        }
        if (appointment.AppointmentDate == default)
        {
            ModelState.AddModelError("AppointmentDate", "Tarih seçimi zorunludur.");
        }
        if (appointment.AppointmentTime == default)
        {
            ModelState.AddModelError("AppointmentTime", "Saat seçimi zorunludur.");
        }

        // SIKI KONTROL: Aynı saatte randevu kontrolü - ÖNCE Antrenör için
        if (appointment.TrainerId > 0 && appointment.AppointmentDate != default && appointment.AppointmentTime != default)
        {
            // Antrenör için kontrol
            var existsAtSameTimeForTrainer = await _appointmentService.ExistsAtSameTimeAsync(
                appointment.TrainerId,
                appointment.AppointmentDate,
                appointment.AppointmentTime);

            if (existsAtSameTimeForTrainer)
            {
                ModelState.AddModelError("AppointmentTime", "Bu saatte antrenör için zaten onaylanmış bir randevu mevcut. Lütfen başka bir saat seçin.");
                TempData["Error"] = "Bu saatte antrenör için zaten onaylanmış bir randevu mevcut. Lütfen başka bir saat seçin.";
                
                ViewBag.MemberId = member.Id;
                ViewBag.Trainers = allTrainers;
                ViewBag.Activities = allActivities;
                ViewBag.GymCenters = allGymCenters;

                return View(appointment);
            }

            // Kullanıcı için kontrol
            var existsAtSameTimeForMember = await _appointmentService.ExistsAtSameTimeForMemberAsync(
                member.Id,
                appointment.AppointmentDate,
                appointment.AppointmentTime);

            if (existsAtSameTimeForMember)
            {
                ModelState.AddModelError("AppointmentTime", "Bu saatte zaten onaylanmış bir randevunuz bulunmaktadır. Onaylı randevunuzun yanında yeni randevu talebi oluşturamazsınız.");
                TempData["Error"] = "Bu saatte zaten onaylanmış bir randevunuz bulunmaktadır. Onaylı randevunuzun yanında yeni randevu talebi oluşturamazsınız.";
                
                ViewBag.MemberId = member.Id;
                ViewBag.Trainers = allTrainers;
                ViewBag.Activities = allActivities;
                ViewBag.GymCenters = allGymCenters;

                return View(appointment);
            }
        }
        
        appointment.Status = AppointmentStatus.Pending;

        // Navigation property'leri temizle
        appointment.Member = null!;
        appointment.Trainer = null!;
        appointment.Activity = null!;
        appointment.GymCenter = null!;

        // ModelState'den navigation property'leri temizle
        ModelState.Remove("Member");
        ModelState.Remove("Trainer");
        ModelState.Remove("Activity");
        ModelState.Remove("GymCenter");

        if (ModelState.IsValid)
        {
            try
            {
                await _appointmentService.CreateAppointmentAsync(appointment);
                TempData["Success"] = "Randevunuz başarıyla oluşturuldu. Onay bekleniyor.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Randevu oluşturulurken hata oluştu: {ex.Message}";
            }
        }

        // Hata durumunda dropdown verilerini tekrar yükle
        ViewBag.MemberId = member.Id;
        ViewBag.Trainers = allTrainers;
        ViewBag.Activities = allActivities;
        ViewBag.GymCenters = allGymCenters;

        return View(appointment);
    }
}

