using Microsoft.AspNetCore.Mvc;
using GymApp.Services;
using GymApp.Repositories;
using GymApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GymApp.Controllers;

// LINQ filtreleri ile çalışan REST API endpoint'lerini sağlayan controller.
[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    // Randevu durumu güncelleme için request model
    public class UpdateAppointmentStatusRequest
    {
        public int Status { get; set; }
    }
    private readonly ITrainerService _trainerService;
    private readonly IAppointmentService _appointmentService;
    private readonly IMemberRepository _memberRepository;
    private readonly GymApp.Data.GymAppDbContext _context;

    // Constructor - gerekli servis ve DbContext bağımlılıklarını alır.
    public ApiController(
        ITrainerService trainerService,
        IAppointmentService appointmentService,
        IMemberRepository memberRepository,
        GymApp.Data.GymAppDbContext context)
    {
        _trainerService = trainerService;
        _appointmentService = appointmentService;
        _memberRepository = memberRepository;
        _context = context;
    }

    // ==========================================================
    // TEMEL VARLIKLAR İÇİN CRUD REST API ENDPOINT'LERİ
    // ==========================================================
    // Category, Product, Event benzeri yapılar için:
    // - Activity  => Event / Product
    // - Trainer   => Personel
    // - GymCenter => Category / Lokasyon
    // - Appointment => Randevu (Event kaydı)

    // GET: /api/api/activities - Tüm aktiviteleri LINQ ile listeler.
    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities()
    {
        // Tüm aktiviteleri GymCenter bilgisiyle birlikte çek
        var activities = await _context.Activities
            .Include(a => a.GymCenter) // Navigation property'yi eager load et
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Description,
                // Enum olarak tutulan aktivite tipini hem Id hem de isim olarak döndür
                TypeId = (int)a.Type, // Enum değerini integer'a çevir
                Type = a.Type.ToString(), // Enum değerini string'e çevir
                a.Duration,
                a.Price,
                a.ImageUrl,
                GymCenterName = a.GymCenter != null ? a.GymCenter.Name : null // Spor salonu adı varsa döndür
            })
            .OrderBy(a => a.Name) // İsme göre alfabetik sırala (dropdown'larda düzenli görünüm için)
            .ToListAsync(); // Asenkron olarak listeye çevir

        // Başarılı response döndür
        return Ok(new { success = true, data = activities, count = activities.Count });
    }

    // GET: /api/api/activities/{id} - Id'ye göre tek bir aktiviteyi döndürür.
    [HttpGet("activities/{id:int}")]
    public async Task<IActionResult> GetActivityById(int id)
    {
        var activity = await _context.Activities
            .Include(a => a.GymCenter)
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Description,
                TypeId = (int)a.Type,
                Type = a.Type.ToString(),
                a.Duration,
                a.Price,
                a.ImageUrl,
                a.GymCenterId,
                GymCenterName = a.GymCenter != null ? a.GymCenter.Name : null
            })
            .FirstOrDefaultAsync();

        if (activity == null)
        {
            return NotFound(new { success = false, message = "Aktivite bulunamadı." });
        }

        return Ok(new { success = true, data = activity });
    }

    // POST: /api/api/activities - Sadece Admin için yeni aktivite oluşturur.
    [HttpPost("activities")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateActivity([FromBody] System.Text.Json.JsonElement jsonElement)
    {
        try
        {
            // Request body'den manuel olarak property'leri oku (camelCase desteği için)
            int gymCenterId = 0;
            if (jsonElement.TryGetProperty("gymCenterId", out var gymCenterIdElement))
            {
                gymCenterId = gymCenterIdElement.GetInt32();
            }
            else if (jsonElement.TryGetProperty("GymCenterId", out var gymCenterIdElement2))
            {
                gymCenterId = gymCenterIdElement2.GetInt32();
            }

            string name = string.Empty;
            if (jsonElement.TryGetProperty("name", out var nameElement))
            {
                name = nameElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Name", out var nameElement2))
            {
                name = nameElement2.GetString() ?? string.Empty;
            }

            string description = string.Empty;
            if (jsonElement.TryGetProperty("description", out var descElement))
            {
                description = descElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Description", out var descElement2))
            {
                description = descElement2.GetString() ?? string.Empty;
            }

            int type = 0;
            if (jsonElement.TryGetProperty("type", out var typeElement))
            {
                type = typeElement.GetInt32();
            }
            else if (jsonElement.TryGetProperty("Type", out var typeElement2))
            {
                type = typeElement2.GetInt32();
            }

            int duration = 0;
            if (jsonElement.TryGetProperty("duration", out var durationElement))
            {
                duration = durationElement.GetInt32();
            }
            else if (jsonElement.TryGetProperty("Duration", out var durationElement2))
            {
                duration = durationElement2.GetInt32();
            }

            decimal price = 0;
            if (jsonElement.TryGetProperty("price", out var priceElement))
            {
                price = priceElement.GetDecimal();
            }
            else if (jsonElement.TryGetProperty("Price", out var priceElement2))
            {
                price = priceElement2.GetDecimal();
            }

            string imageUrl = string.Empty;
            if (jsonElement.TryGetProperty("imageUrl", out var imageUrlElement))
            {
                imageUrl = imageUrlElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("ImageUrl", out var imageUrlElement2))
            {
                imageUrl = imageUrlElement2.GetString() ?? string.Empty;
            }

            // Validasyon
            if (gymCenterId <= 0)
            {
                return BadRequest(new { success = false, message = "Geçerli bir spor salonu seçiniz." });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { success = false, message = "Aktivite adı zorunludur." });
            }

            // Description boşsa varsayılan değer ata
            if (string.IsNullOrWhiteSpace(description))
            {
                description = "Açıklama bulunmamaktadır.";
            }

            if (type <= 0)
            {
                return BadRequest(new { success = false, message = "Aktivite tipi zorunludur." });
            }

            // ActivityType enum kontrolü
            if (!Enum.IsDefined(typeof(ActivityType), type))
            {
                return BadRequest(new { success = false, message = $"Geçersiz aktivite tipi: {type}" });
            }

            if (duration <= 0)
            {
                return BadRequest(new { success = false, message = "Süre 1 dakikadan fazla olmalıdır." });
            }

            if (price < 0)
            {
                return BadRequest(new { success = false, message = "Fiyat 0 veya daha büyük olmalıdır." });
            }

            // GymCenterId kontrolü
            var gymCenterExists = await _context.GymCenters.AnyAsync(gc => gc.Id == gymCenterId);
            if (!gymCenterExists)
            {
                return BadRequest(new { success = false, message = $"Geçersiz GymCenterId: {gymCenterId}" });
            }

            // Yeni Activity oluştur (navigation property'ler olmadan)
            var newActivity = new Activity
            {
                GymCenterId = gymCenterId,
                Name = name,
                Description = description,
                Type = (ActivityType)type,
                Duration = duration,
                Price = price,
                ImageUrl = imageUrl
            };

            _context.Activities.Add(newActivity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetActivityById), new { id = newActivity.Id }, new { success = true, data = newActivity });
        }
        catch (Exception ex)
        {
            // Detaylı hata mesajı için inner exception'ı da kontrol et
            var errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += " | Inner: " + ex.InnerException.Message;
            }
            return StatusCode(500, new { success = false, message = $"Hata: {errorMessage}" });
        }
    }

    // PUT: /api/api/activities/{id} - Sadece Admin için aktivite günceller.
    [HttpPut("activities/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateActivity(int id, [FromBody] Activity model)
    {
        try
        {
            if (id != model.Id)
            {
                return BadRequest(new { success = false, message = "ID uyuşmuyor." });
            }

            // GymCenterId kontrolü ÖNCE
            if (model.GymCenterId <= 0)
            {
                return BadRequest(new { success = false, message = "Geçerli bir spor salonu seçiniz." });
            }

            var gymCenterExists = await _context.GymCenters.AnyAsync(gc => gc.Id == model.GymCenterId);
            if (!gymCenterExists)
            {
                return BadRequest(new { success = false, message = $"Geçersiz GymCenterId: {model.GymCenterId}" });
            }

            var existingActivity = await _context.Activities.FindAsync(id);
            if (existingActivity == null)
            {
                return NotFound(new { success = false, message = "Aktivite bulunamadı." });
            }

            // Navigation property'leri ModelState'den temizle
            ModelState.Remove("GymCenter");
            ModelState.Remove("TrainerActivities");
            ModelState.Remove("Appointments");
            ModelState.Remove("Id");
            
            // ModelState'deki tüm GymCenter ile ilgili hataları temizle (case-insensitive)
            var keysToRemove = ModelState.Keys
                .Where(k => k.Contains("GymCenter", StringComparison.OrdinalIgnoreCase) || 
                           k.Contains("TrainerActivities", StringComparison.OrdinalIgnoreCase) || 
                           k.Contains("Appointments", StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage ?? "Hata").ToArray()
                    );
                return BadRequest(new { success = false, errors = errors, message = "Model doğrulama hatası. Lütfen tüm zorunlu alanları doldurun." });
            }

            // Property'leri güncelle (navigation property'leri dokunmadan)
            existingActivity.GymCenterId = model.GymCenterId;
            existingActivity.Name = model.Name;
            existingActivity.Description = model.Description;
            existingActivity.Type = model.Type;
            existingActivity.Duration = model.Duration;
            existingActivity.Price = model.Price;
            existingActivity.ImageUrl = model.ImageUrl ?? string.Empty;

            _context.Activities.Update(existingActivity);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, data = existingActivity });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // DELETE: /api/api/activities/{id} - Sadece Admin için aktivite siler.
    [HttpDelete("activities/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null)
        {
            return NotFound(new { success = false, message = "Aktivite bulunamadı." });
        }

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // GET: /api/api/trainers - Tüm antrenörleri detaylarıyla birlikte listeler.
    [HttpGet("trainers")]
    public async Task<IActionResult> GetAllTrainers()
    {
        try
        {
            // LINQ sorgusu: Tüm antrenörleri çalışma saatleri ve spor salonu bilgileri ile birlikte getir
            var trainers = await _context.Trainers
                .Include(t => t.GymCenter) // Spor salonu bilgisini dahil et
                .Include(t => t.TrainerActivities) // Antrenör aktivitelerini dahil et
                    .ThenInclude(ta => ta.Activity) // Aktivite detaylarını dahil et
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.Phone,
                    t.Specialization,
                    t.ProfilePhotoUrl,
                    t.WorkingHoursJson,
                    t.GymCenterId,
                    GymCenterName = t.GymCenter != null ? t.GymCenter.Name : null,
                    Activities = t.TrainerActivities.Select(ta => new
                    {
                        ta.Activity.Id,
                        ta.Activity.Name,
                        ta.Activity.Price
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { success = true, data = trainers, count = trainers.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // GET: /api/api/available-trainers - Belirli tarih/saat için uygun antrenörleri döndürür.
    [HttpGet("available-trainers")]
    public async Task<IActionResult> GetAvailableTrainers([FromQuery] string? date, [FromQuery] string? time)
    {
        try
        {
            // Tarih ve saat parametrelerini kontrol et
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                return BadRequest(new { success = false, message = "Tarih ve saat parametreleri gereklidir." });
            }

            // Tarih ve saat formatını parse et
            if (!DateTime.TryParse(date, out DateTime appointmentDate))
            {
                return BadRequest(new { success = false, message = "Geçersiz tarih formatı. Format: yyyy-MM-dd" });
            }

            if (!TimeSpan.TryParse(time, out TimeSpan appointmentTime))
            {
                return BadRequest(new { success = false, message = "Geçersiz saat formatı. Format: HH:mm" });
            }

            // Haftanın gününü bul (0 = Pazar, 1 = Pazartesi, ...)
            int dayOfWeek = (int)appointmentDate.DayOfWeek;

            // LINQ sorgusu: Belirli tarih ve saatte uygun antrenörleri bul
            var availableTrainers = await _context.Trainers
                .Include(t => t.GymCenter)
                .Include(t => t.WorkingHours)
                .Where(t => t.WorkingHours.Any(wh => (int)wh.DayOfWeek == dayOfWeek)) // Çalışma günü kontrolü (DayOfWeek enum'u int'e cast edilir)
                .Where(t => !_context.Appointments
                    .Any(a => a.TrainerId == t.Id &&
                             a.AppointmentDate.Date == appointmentDate.Date &&
                             a.AppointmentTime.Hours == appointmentTime.Hours &&
                             a.AppointmentTime.Minutes == appointmentTime.Minutes &&
                             a.Status == AppointmentStatus.Approved)) // Onaylı randevu kontrolü
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.Phone,
                    t.Specialization,
                    t.ProfilePhotoUrl,
                    GymCenterName = t.GymCenter != null ? t.GymCenter.Name : null,
                    WorkingHours = t.WorkingHours
                        .Where(wh => (int)wh.DayOfWeek == dayOfWeek)
                        .Select(wh => new 
                        { 
                            DayOfWeek = wh.DayOfWeek.ToString(),
                            StartTime = wh.StartTime.ToString(@"hh\:mm"),
                            EndTime = wh.EndTime.ToString(@"hh\:mm")
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(new 
            { 
                success = true, 
                data = availableTrainers, 
                count = availableTrainers.Count,
                date = appointmentDate.ToString("yyyy-MM-dd"),
                time = appointmentTime.ToString(@"hh\:mm"),
                dayOfWeek = dayOfWeek
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // GET: /api/api/appointments - Tüm randevuları veya filtreli randevuları listeler.
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAllAppointments([FromQuery] int? memberId, [FromQuery] string? status)
    {
        try
        {
            // LINQ sorgusu: Randevuları filtrele
            var query = _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.GymCenter)
                .Include(a => a.Activity)
                .Include(a => a.GymCenter)
                .AsQueryable();

            // Üye filtresi
            if (memberId.HasValue)
            {
                query = query.Where(a => a.MemberId == memberId.Value);
            }

            // Durum filtresi
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(a => a.Status == statusEnum);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Geçersiz durum değeri." });
                }
            }

            // Randevuları tarihe göre sırala ve detaylı bilgileri getir
            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
                    Status = (int)a.Status, // Enum'u integer'a çevir
                    a.Price,
                    Member = new
                    {
                        a.Member.Id,
                        a.Member.FirstName,
                        a.Member.LastName,
                        a.Member.Email
                    },
                    Trainer = new
                    {
                        a.Trainer.Id,
                        a.Trainer.FirstName,
                        a.Trainer.LastName,
                        a.Trainer.Email,
                        GymCenterName = a.Trainer.GymCenter != null ? a.Trainer.GymCenter.Name : null
                    },
                    Activity = new
                    {
                        a.Activity.Id,
                        a.Activity.Name,
                        a.Activity.Price
                    },
                    GymCenter = new
                    {
                        a.GymCenter.Id,
                        a.GymCenter.Name
                    }
                })
                .ToListAsync();

            return Ok(new 
            { 
                success = true, 
                data = appointments, 
                count = appointments.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // GET: /api/api/member-appointments - Üye ve duruma göre randevuları listeler.
    [HttpGet("member-appointments")]
    public async Task<IActionResult> GetMemberAppointments([FromQuery] int? memberId, [FromQuery] string? status)
    {
        try
        {
            if (!memberId.HasValue)
            {
                return BadRequest(new { success = false, message = "memberId parametresi gereklidir." });
            }

            // Üyenin var olup olmadığını kontrol et
            var member = await _memberRepository.GetByIdAsync(memberId.Value);
            if (member == null)
            {
                return NotFound(new { success = false, message = "Üye bulunamadı." });
            }

            // LINQ sorgusu: Üye randevularını filtrele
            var query = _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.GymCenter)
                .Include(a => a.Activity)
                .Include(a => a.Member)
                .Where(a => a.MemberId == memberId.Value)
                .AsQueryable();

            // Durum filtresi varsa uygula
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<AppointmentStatus>(status, true, out AppointmentStatus statusEnum))
                {
                    query = query.Where(a => a.Status == statusEnum);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Geçersiz durum değeri. Geçerli değerler: OnayBekliyor, Onaylandi, Reddedildi" });
                }
            }

            // Randevuları tarihe göre sırala ve detaylı bilgileri getir
            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
                    a.Status,
                    a.Price,
                    Trainer = new
                    {
                        a.Trainer.Id,
                        a.Trainer.FirstName,
                        a.Trainer.LastName,
                        a.Trainer.Email,
                        GymCenterName = a.Trainer.GymCenter != null ? a.Trainer.GymCenter.Name : null
                    },
                    Activity = new
                    {
                        a.Activity.Id,
                        a.Activity.Name,
                        a.Activity.Price
                    },
                    Member = new
                    {
                        a.Member.Id,
                        a.Member.FirstName,
                        a.Member.LastName,
                        a.Member.Email
                    }
                })
                .ToListAsync();

            return Ok(new 
            { 
                success = true, 
                data = appointments, 
                count = appointments.Count,
                memberId = memberId.Value,
                memberName = $"{member.FirstName} {member.LastName}",
                statusFilter = status
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // GET: /api/api/trainers-by-specialization - Uzmanlık alanına göre antrenörleri filtreler.
    [HttpGet("trainers-by-specialization")]
    public async Task<IActionResult> GetTrainersBySpecialization([FromQuery] string? specialization)
    {
        try
        {
            if (string.IsNullOrEmpty(specialization))
            {
                return BadRequest(new { success = false, message = "specialization parametresi gereklidir." });
            }

            // LINQ sorgusu: Uzmanlık alanına göre antrenörleri filtrele
            var trainers = await _context.Trainers
                .Include(t => t.GymCenter)
                .Include(t => t.TrainerActivities)
                    .ThenInclude(ta => ta.Activity)
                .Where(t => t.Specialization != null && t.Specialization.Contains(specialization))
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.Phone,
                    t.Specialization,
                    t.ProfilePhotoUrl,
                    GymCenterName = t.GymCenter != null ? t.GymCenter.Name : null,
                    Activities = t.TrainerActivities.Select(ta => new
                    {
                        ta.Activity.Id,
                        ta.Activity.Name,
                        ta.Activity.Price
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new 
            { 
                success = true, 
                data = trainers, 
                count = trainers.Count,
                specialization = specialization
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // GET: /api/api/gym-centers-by-activity - Belirli aktiviteye sahip spor salonlarını filtreler.
    [HttpGet("gym-centers-by-activity")]
    public async Task<IActionResult> GetGymCentersByActivity([FromQuery] int? activityId, [FromQuery] int? dayOfWeek = null)
    {
        try
        {
            if (!activityId.HasValue)
            {
                return BadRequest(new { success = false, message = "activityId parametresi gereklidir." });
            }

            // LINQ sorgusu: Belirli aktiviteye sahip spor salonlarını filtrele
            var query = _context.GymCenters
                .Include(g => g.Activities)
                .Where(g => g.Activities.Any(a => a.Id == activityId.Value))
                .AsQueryable();

            // Çalışma günü filtresi (WorkingHoursJson'dan parse edilir)
            if (dayOfWeek.HasValue)
            {
                // JSON string içinde "Day":X formatını ara (X = dayOfWeek değeri)
                // Örnek: [{"Day":1,"TimeRange":"09:00-18:00"}] formatında "Day":1 aranır
                var dayPattern = $"\"Day\":{dayOfWeek.Value}";
                query = query.Where(g => !string.IsNullOrEmpty(g.WorkingHoursJson) && 
                    g.WorkingHoursJson != "[]" &&
                    (g.WorkingHoursJson.Contains(dayPattern) || 
                     g.WorkingHoursJson.Contains($"\"Day\": {dayOfWeek.Value}"))); // Boşluklu versiyon da kontrol et
            }

            var gymCenters = await query
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Address,
                    g.Phone,
                    g.Email,
                    g.Description,
                    g.ImageUrl,
                    g.WorkingHoursJson,
                    Activities = g.Activities.Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Price,
                        a.Type
                    }).ToList(),
                    ActivityCount = g.Activities.Count
                })
                .ToListAsync();

            return Ok(new 
            { 
                success = true, 
                data = gymCenters, 
                count = gymCenters.Count,
                activityId = activityId.Value,
                dayOfWeek = dayOfWeek
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // PUT: /api/api/appointments/{id}/status - Randevu durumunu günceller (Admin).
    [HttpPut("appointments/{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new { success = false, message = "Randevu bulunamadı." });
            }

            if (Enum.TryParse<AppointmentStatus>(request.Status.ToString(), out var status))
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
                return Ok(new { success = true, data = appointment });
            }
            else
            {
                return BadRequest(new { success = false, message = "Geçersiz durum değeri." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // DELETE: /api/api/appointments/{id} - Randevu siler (Admin).
    [HttpDelete("appointments/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new { success = false, message = "Randevu bulunamadı." });
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // ==========================================================
    // TRAINER CRUD API ENDPOINT'LERİ
    // ==========================================================

    // GET: /api/api/trainers/{id} - Id'ye göre tek bir antrenörü döndürür.
    [HttpGet("trainers/{id:int}")]
    public async Task<IActionResult> GetTrainerById(int id)
    {
        try
        {
            var trainer = await _context.Trainers
                .Include(t => t.GymCenter)
                .Include(t => t.TrainerActivities)
                    .ThenInclude(ta => ta.Activity)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.Phone,
                    t.Specialization,
                    t.ProfilePhotoUrl,
                    t.GymCenterId,
                    t.WorkingHoursJson,
                    GymCenterName = t.GymCenter != null ? t.GymCenter.Name : null,
                    Activities = t.TrainerActivities.Select(ta => new
                    {
                        ta.Activity.Id,
                        ta.Activity.Name,
                        ta.Activity.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (trainer == null)
            {
                return NotFound(new { success = false, message = "Antrenör bulunamadı." });
            }

            return Ok(new { success = true, data = trainer });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // POST: /api/api/trainers - Sadece Admin için yeni antrenör oluşturur.
    [HttpPost("trainers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTrainer([FromBody] System.Text.Json.JsonElement jsonElement)
    {
        try
        {
            // Request body'den manuel olarak property'leri oku (camelCase desteği için)
            string firstName = string.Empty;
            if (jsonElement.TryGetProperty("firstName", out var firstNameElement))
            {
                firstName = firstNameElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("FirstName", out var firstNameElement2))
            {
                firstName = firstNameElement2.GetString() ?? string.Empty;
            }

            string lastName = string.Empty;
            if (jsonElement.TryGetProperty("lastName", out var lastNameElement))
            {
                lastName = lastNameElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("LastName", out var lastNameElement2))
            {
                lastName = lastNameElement2.GetString() ?? string.Empty;
            }

            string email = string.Empty;
            if (jsonElement.TryGetProperty("email", out var emailElement))
            {
                email = emailElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Email", out var emailElement2))
            {
                email = emailElement2.GetString() ?? string.Empty;
            }

            string phone = string.Empty;
            if (jsonElement.TryGetProperty("phone", out var phoneElement))
            {
                phone = phoneElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Phone", out var phoneElement2))
            {
                phone = phoneElement2.GetString() ?? string.Empty;
            }

            string profilePhotoUrl = string.Empty;
            if (jsonElement.TryGetProperty("profilePhotoUrl", out var photoElement))
            {
                profilePhotoUrl = photoElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("ProfilePhotoUrl", out var photoElement2))
            {
                profilePhotoUrl = photoElement2.GetString() ?? string.Empty;
            }

            int gymCenterId = 0;
            if (jsonElement.TryGetProperty("gymCenterId", out var gymCenterIdElement))
            {
                gymCenterId = gymCenterIdElement.GetInt32();
            }
            else if (jsonElement.TryGetProperty("GymCenterId", out var gymCenterIdElement2))
            {
                gymCenterId = gymCenterIdElement2.GetInt32();
            }

            string specialization = string.Empty;
            if (jsonElement.TryGetProperty("specialization", out var specElement))
            {
                specialization = specElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Specialization", out var specElement2))
            {
                specialization = specElement2.GetString() ?? string.Empty;
            }

            string password = string.Empty;
            if (jsonElement.TryGetProperty("password", out var passwordElement))
            {
                password = passwordElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Password", out var passwordElement2))
            {
                password = passwordElement2.GetString() ?? string.Empty;
            }

            string workingHoursJson = "[]";
            if (jsonElement.TryGetProperty("workingHoursJson", out var whElement))
            {
                workingHoursJson = whElement.GetString() ?? "[]";
            }
            else if (jsonElement.TryGetProperty("WorkingHoursJson", out var whElement2))
            {
                workingHoursJson = whElement2.GetString() ?? "[]";
            }

            // TrainerActivities - opsiyonel
            List<int> activityIds = new List<int>();
            if (jsonElement.TryGetProperty("trainerActivities", out var taElement) && taElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in taElement.EnumerateArray())
                {
                    if (item.TryGetProperty("activityId", out var actIdElement))
                    {
                        activityIds.Add(actIdElement.GetInt32());
                    }
                    else if (item.TryGetProperty("ActivityId", out var actIdElement2))
                    {
                        activityIds.Add(actIdElement2.GetInt32());
                    }
                }
            }
            else if (jsonElement.TryGetProperty("TrainerActivities", out var taElement2) && taElement2.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in taElement2.EnumerateArray())
                {
                    if (item.TryGetProperty("activityId", out var actIdElement))
                    {
                        activityIds.Add(actIdElement.GetInt32());
                    }
                    else if (item.TryGetProperty("ActivityId", out var actIdElement2))
                    {
                        activityIds.Add(actIdElement2.GetInt32());
                    }
                }
            }

            // Validasyon
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return BadRequest(new { success = false, message = "Ad alanı zorunludur." });
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                return BadRequest(new { success = false, message = "Soyad alanı zorunludur." });
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { success = false, message = "E-posta alanı zorunludur." });
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                return BadRequest(new { success = false, message = "Şifre en az 6 karakter olmalıdır." });
            }

            if (gymCenterId <= 0)
            {
                return BadRequest(new { success = false, message = "Geçerli bir spor salonu seçiniz." });
            }

            // GymCenterId kontrolü
            var gymCenterExists = await _context.GymCenters.AnyAsync(gc => gc.Id == gymCenterId);
            if (!gymCenterExists)
            {
                return BadRequest(new { success = false, message = $"Geçersiz GymCenterId: {gymCenterId}" });
            }

            // Specialization boşsa aktivitelerden oluştur
            if (string.IsNullOrWhiteSpace(specialization) && activityIds.Any())
            {
                var activities = await _context.Activities
                    .Where(a => activityIds.Contains(a.Id))
                    .Select(a => a.Name)
                    .ToListAsync();
                specialization = activities.Any() ? string.Join(", ", activities) : "Belirtilmemiş";
            }
            else if (string.IsNullOrWhiteSpace(specialization))
            {
                specialization = "Belirtilmemiş";
            }

            // WorkingHoursJson kontrolü
            if (string.IsNullOrWhiteSpace(workingHoursJson))
            {
                workingHoursJson = "[]";
            }

            // Yeni Trainer oluştur
            var newTrainer = new Trainer
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                ProfilePhotoUrl = profilePhotoUrl,
                GymCenterId = gymCenterId,
                Specialization = specialization,
                Password = password,
                WorkingHoursJson = workingHoursJson
            };

            _context.Trainers.Add(newTrainer);
            await _context.SaveChangesAsync();

            // TrainerActivities'i kaydet
            if (activityIds.Any())
            {
                var validActivityIds = await _context.Activities
                    .Where(a => activityIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync();

                var uniqueActivityIds = activityIds
                    .Distinct()
                    .Where(id => validActivityIds.Contains(id))
                    .ToList();

                var validTrainerActivities = uniqueActivityIds.Select(activityId => new TrainerActivity
                {
                    TrainerId = newTrainer.Id,
                    ActivityId = activityId,
                    Trainer = null!,
                    Activity = null!
                }).ToList();

                if (validTrainerActivities.Any())
                {
                    await _context.TrainerActivities.AddRangeAsync(validTrainerActivities);
                    await _context.SaveChangesAsync();
                }
            }

            return CreatedAtAction(nameof(GetTrainerById), new { id = newTrainer.Id }, new { success = true, data = newTrainer });
        }
        catch (Exception ex)
        {
            // Detaylı hata mesajı için inner exception'ı da kontrol et
            var errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += " | Inner: " + ex.InnerException.Message;
            }
            return StatusCode(500, new { success = false, message = $"Hata: {errorMessage}" });
        }
    }

    // PUT: /api/api/trainers/{id} - Sadece Admin için antrenör günceller.
    [HttpPut("trainers/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTrainer(int id, [FromBody] System.Text.Json.JsonElement jsonElement)
    {
        try
        {
            // Request body'den manuel olarak property'leri oku (camelCase desteği için)
            int trainerId = 0;
            if (jsonElement.TryGetProperty("id", out var idElement))
            {
                trainerId = idElement.GetInt32();
            }
            else if (jsonElement.TryGetProperty("Id", out var idElement2))
            {
                trainerId = idElement2.GetInt32();
            }

            if (id != trainerId)
            {
                return BadRequest(new { success = false, message = "ID uyuşmuyor." });
            }

            // Önce mevcut antrenörü al (şifreyi korumak için)
            var existingTrainer = await _context.Trainers
                .Include(t => t.TrainerActivities)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTrainer == null)
            {
                return NotFound(new { success = false, message = "Antrenör bulunamadı." });
            }

            // Request body'den property'leri oku
            string firstName = string.Empty;
            if (jsonElement.TryGetProperty("firstName", out var firstNameElement))
            {
                firstName = firstNameElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("FirstName", out var firstNameElement2))
            {
                firstName = firstNameElement2.GetString() ?? string.Empty;
            }

            string lastName = string.Empty;
            if (jsonElement.TryGetProperty("lastName", out var lastNameElement))
            {
                lastName = lastNameElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("LastName", out var lastNameElement2))
            {
                lastName = lastNameElement2.GetString() ?? string.Empty;
            }

            string email = string.Empty;
            if (jsonElement.TryGetProperty("email", out var emailElement))
            {
                email = emailElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Email", out var emailElement2))
            {
                email = emailElement2.GetString() ?? string.Empty;
            }

            string phone = string.Empty;
            if (jsonElement.TryGetProperty("phone", out var phoneElement))
            {
                phone = phoneElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Phone", out var phoneElement2))
            {
                phone = phoneElement2.GetString() ?? string.Empty;
            }

            string profilePhotoUrl = string.Empty;
            if (jsonElement.TryGetProperty("profilePhotoUrl", out var photoElement))
            {
                profilePhotoUrl = photoElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("ProfilePhotoUrl", out var photoElement2))
            {
                profilePhotoUrl = photoElement2.GetString() ?? string.Empty;
            }

            int gymCenterId = 0;
            if (jsonElement.TryGetProperty("gymCenterId", out var gymCenterIdElement))
            {
                gymCenterId = gymCenterIdElement.GetInt32();
            }
            else if (jsonElement.TryGetProperty("GymCenterId", out var gymCenterIdElement2))
            {
                gymCenterId = gymCenterIdElement2.GetInt32();
            }

            string specialization = string.Empty;
            if (jsonElement.TryGetProperty("specialization", out var specElement))
            {
                specialization = specElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Specialization", out var specElement2))
            {
                specialization = specElement2.GetString() ?? string.Empty;
            }

            string workingHoursJson = "[]";
            if (jsonElement.TryGetProperty("workingHoursJson", out var whElement))
            {
                workingHoursJson = whElement.GetString() ?? "[]";
            }
            else if (jsonElement.TryGetProperty("WorkingHoursJson", out var whElement2))
            {
                workingHoursJson = whElement2.GetString() ?? "[]";
            }

            // Password - Antrenörlerde şifre güncellemesi yok, bu alanı ignore ediyoruz

            // TrainerActivities - opsiyonel
            List<int> activityIds = new List<int>();
            if (jsonElement.TryGetProperty("trainerActivities", out var taElement) && taElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in taElement.EnumerateArray())
                {
                    if (item.TryGetProperty("activityId", out var actIdElement))
                    {
                        activityIds.Add(actIdElement.GetInt32());
                    }
                    else if (item.TryGetProperty("ActivityId", out var actIdElement2))
                    {
                        activityIds.Add(actIdElement2.GetInt32());
                    }
                }
            }
            else if (jsonElement.TryGetProperty("TrainerActivities", out var taElement2) && taElement2.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in taElement2.EnumerateArray())
                {
                    if (item.TryGetProperty("activityId", out var actIdElement))
                    {
                        activityIds.Add(actIdElement.GetInt32());
                    }
                    else if (item.TryGetProperty("ActivityId", out var actIdElement2))
                    {
                        activityIds.Add(actIdElement2.GetInt32());
                    }
                }
            }

            // Validasyon
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return BadRequest(new { success = false, message = "Ad alanı zorunludur." });
            }

            if (gymCenterId <= 0)
            {
                return BadRequest(new { success = false, message = "Geçerli bir spor salonu seçiniz." });
            }

            // GymCenterId kontrolü
            var gymCenterExists = await _context.GymCenters.AnyAsync(gc => gc.Id == gymCenterId);
            if (!gymCenterExists)
            {
                return BadRequest(new { success = false, message = $"Geçersiz GymCenterId: {gymCenterId}" });
            }

            // Specialization boşsa aktivitelerden oluştur
            if (string.IsNullOrWhiteSpace(specialization) && activityIds.Any())
            {
                var activities = await _context.Activities
                    .Where(a => activityIds.Contains(a.Id))
                    .Select(a => a.Name)
                    .ToListAsync();
                specialization = activities.Any() ? string.Join(", ", activities) : "Belirtilmemiş";
            }
            else if (string.IsNullOrWhiteSpace(specialization))
            {
                specialization = "Belirtilmemiş";
            }

            // WorkingHoursJson kontrolü
            if (string.IsNullOrWhiteSpace(workingHoursJson))
            {
                workingHoursJson = "[]";
            }

            // Mevcut TrainerActivities'i sil
            var existingTrainerActivities = await _context.TrainerActivities
                .Where(ta => ta.TrainerId == id)
                .ToListAsync();
            _context.TrainerActivities.RemoveRange(existingTrainerActivities);

            // Property'leri güncelle
            existingTrainer.FirstName = firstName;
            existingTrainer.LastName = lastName;
            existingTrainer.Email = email;
            existingTrainer.Phone = phone;
            existingTrainer.GymCenterId = gymCenterId;
            existingTrainer.Specialization = specialization;
            existingTrainer.WorkingHoursJson = workingHoursJson;
            existingTrainer.ProfilePhotoUrl = profilePhotoUrl;

            // Password - Antrenörlerde şifre güncellemesi yapılmıyor, mevcut şifre korunuyor

            // Yeni TrainerActivities'i ekle
            if (activityIds.Any())
            {
                var validActivityIds = await _context.Activities
                    .Where(a => activityIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync();

                var uniqueActivityIds = activityIds
                    .Distinct()
                    .Where(id => validActivityIds.Contains(id))
                    .ToList();

                var validTrainerActivities = uniqueActivityIds.Select(activityId => new TrainerActivity
                {
                    TrainerId = id,
                    ActivityId = activityId,
                    Trainer = null!,
                    Activity = null!
                }).ToList();

                if (validTrainerActivities.Any())
                {
                    await _context.TrainerActivities.AddRangeAsync(validTrainerActivities);
                }
            }

            _context.Trainers.Update(existingTrainer);
            await _context.SaveChangesAsync();

            // Response için sadece gerekli property'leri döndür (navigation property'ler olmadan)
            var responseData = new
            {
                existingTrainer.Id,
                existingTrainer.FirstName,
                existingTrainer.LastName,
                existingTrainer.Email,
                existingTrainer.Phone,
                existingTrainer.ProfilePhotoUrl,
                existingTrainer.GymCenterId,
                existingTrainer.Specialization,
                existingTrainer.WorkingHoursJson
            };

            return Ok(new { success = true, data = responseData });
        }
        catch (Exception ex)
        {
            // Detaylı hata mesajı için inner exception'ı da kontrol et
            var errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += " | Inner: " + ex.InnerException.Message;
            }
            // Stack trace'i de ekle (debug için)
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                var stackTraceLength = Math.Min(500, ex.StackTrace.Length);
                errorMessage += " | StackTrace: " + ex.StackTrace.Substring(0, stackTraceLength);
            }
            return StatusCode(500, new { success = false, message = $"Hata: {errorMessage}" });
        }
    }

    // DELETE: /api/api/trainers/{id} - Sadece Admin için antrenör siler.
    [HttpDelete("trainers/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTrainer(int id)
    {
        try
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound(new { success = false, message = "Antrenör bulunamadı." });
            }

            _context.Trainers.Remove(trainer);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // ==========================================================
    // GYMCENTER CRUD API ENDPOINT'LERİ
    // ==========================================================

    // GET: /api/api/gym-centers - Tüm spor salonlarını LINQ ile listeler.
    [HttpGet("gym-centers")]
    public async Task<IActionResult> GetAllGymCenters([FromQuery] int? dayOfWeek = null)
    {
        try
        {
            var query = _context.GymCenters
                .Include(g => g.Activities)
                .AsQueryable();

            // Çalışma günü filtresi (WorkingHoursJson'dan parse edilir)
            if (dayOfWeek.HasValue)
            {
                // JSON string içinde "Day":X formatını ara (X = dayOfWeek değeri)
                // Örnek: [{"Day":1,"TimeRange":"09:00-18:00"}] formatında "Day":1 aranır
                var dayPattern = $"\"Day\":{dayOfWeek.Value}";
                query = query.Where(g => !string.IsNullOrEmpty(g.WorkingHoursJson) && 
                    g.WorkingHoursJson != "[]" &&
                    (g.WorkingHoursJson.Contains(dayPattern) || 
                     g.WorkingHoursJson.Contains($"\"Day\": {dayOfWeek.Value}"))); // Boşluklu versiyon da kontrol et
            }

            var gymCenters = await query
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Description,
                    g.Address,
                    g.Phone,
                    g.Email,
                    g.ImageUrl,
                    g.IsActive,
                    g.WorkingHoursJson,
                    ActivityCount = g.Activities.Count
                })
                .ToListAsync();

            return Ok(new { success = true, data = gymCenters, count = gymCenters.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // GET: /api/api/gym-centers/{id} - Id'ye göre tek bir spor salonunu döndürür.
    [HttpGet("gym-centers/{id:int}")]
    public async Task<IActionResult> GetGymCenterById(int id)
    {
        try
        {
            var gymCenter = await _context.GymCenters
                .Include(g => g.Activities)
                .Where(g => g.Id == id)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Description,
                    g.Address,
                    g.Phone,
                    g.Email,
                    g.ImageUrl,
                    g.IsActive,
                    g.WorkingHoursJson,
                    Activities = g.Activities.Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Price,
                        a.Type
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (gymCenter == null)
            {
                return NotFound(new { success = false, message = "Spor salonu bulunamadı." });
            }

            return Ok(new { success = true, data = gymCenter });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // POST: /api/api/gym-centers - Sadece Admin için yeni spor salonu oluşturur.
    [HttpPost("gym-centers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateGymCenter([FromBody] GymCenter model)
    {
        try
        {
            // Navigation property'leri null yap (validation hatası vermemesi için)
            model.Activities = new List<Activity>();
            model.Photos = new List<GymCenterPhoto>();
            model.Trainers = new List<Trainer>();
            model.WorkingHoursList = new List<GymCenterWorkingHours>();

            // Navigation property'leri ModelState'den temizle
            ModelState.Remove("Activities");
            ModelState.Remove("Photos");
            ModelState.Remove("Trainers");
            ModelState.Remove("WorkingHoursList");

            if (string.IsNullOrWhiteSpace(model.WorkingHoursJson))
            {
                model.WorkingHoursJson = "[]";
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage ?? "Hata").ToArray()
                    );
                return BadRequest(new { success = false, errors = errors, message = "Model doğrulama hatası. Lütfen tüm zorunlu alanları doldurun." });
            }

            _context.GymCenters.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGymCenterById), new { id = model.Id }, new { success = true, data = model });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // PUT: /api/api/gym-centers/{id} - Sadece Admin için spor salonu günceller.
    [HttpPut("gym-centers/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateGymCenter(int id, [FromBody] GymCenter model)
    {
        try
        {
            if (id != model.Id)
            {
                return BadRequest(new { success = false, message = "ID uyuşmuyor." });
            }

            // Navigation property'leri null yap (validation hatası vermemesi için)
            model.Activities = new List<Activity>();
            model.Photos = new List<GymCenterPhoto>();
            model.Trainers = new List<Trainer>();
            model.WorkingHoursList = new List<GymCenterWorkingHours>();

            // Navigation property'leri ModelState'den temizle
            ModelState.Remove("Activities");
            ModelState.Remove("Photos");
            ModelState.Remove("Trainers");
            ModelState.Remove("WorkingHoursList");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage ?? "Hata").ToArray()
                    );
                return BadRequest(new { success = false, errors = errors, message = "Model doğrulama hatası. Lütfen tüm zorunlu alanları doldurun." });
            }

            var existingGymCenter = await _context.GymCenters
                .FirstOrDefaultAsync(g => g.Id == id);

            if (existingGymCenter == null)
            {
                return NotFound(new { success = false, message = "Spor salonu bulunamadı." });
            }

            // Property'leri güncelle
            existingGymCenter.Name = model.Name;
            existingGymCenter.Description = model.Description ?? string.Empty;
            existingGymCenter.Address = model.Address;
            existingGymCenter.Phone = model.Phone;
            existingGymCenter.Email = model.Email;
            existingGymCenter.ImageUrl = model.ImageUrl ?? string.Empty;
            existingGymCenter.IsActive = model.IsActive;
            existingGymCenter.WorkingHoursJson = string.IsNullOrWhiteSpace(model.WorkingHoursJson) ? "[]" : model.WorkingHoursJson;
            existingGymCenter.Advertisement = model.Advertisement ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, data = existingGymCenter });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // DELETE: /api/api/gym-centers/{id} - Sadece Admin için spor salonu siler.
    [HttpDelete("gym-centers/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteGymCenter(int id)
    {
        try
        {
            var gymCenter = await _context.GymCenters.FindAsync(id);
            if (gymCenter == null)
            {
                return NotFound(new { success = false, message = "Spor salonu bulunamadı." });
            }

            _context.GymCenters.Remove(gymCenter);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // ==========================================================
    // MEMBER (ÜYE) API ENDPOINT'LERİ
    // ==========================================================

    // GET: /api/api/members - Tüm üyeleri LINQ ile listeler (Admin için).
    [HttpGet("members")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllMembers()
    {
        try
        {
            var members = await _context.Members
                .OrderByDescending(m => m.RegistrationDate)
                .Select(m => new
                {
                    m.Id,
                    m.FirstName,
                    m.LastName,
                    m.Email,
                    m.Phone,
                    m.RegistrationDate
                })
                .ToListAsync();

            return Ok(new { success = true, data = members, count = members.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // GET: /api/api/members/{id} - Belirli bir üyeyi getirir (Admin için).
    [HttpGet("members/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMemberById(int id)
    {
        try
        {
            var member = await _context.Members
                .Where(m => m.Id == id)
                .Select(m => new
                {
                    m.Id,
                    m.FirstName,
                    m.LastName,
                    m.Email,
                    m.Phone,
                    m.RegistrationDate
                })
                .FirstOrDefaultAsync();

            if (member == null)
            {
                return NotFound(new { success = false, message = "Üye bulunamadı." });
            }

            return Ok(new { success = true, data = member });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // POST: /api/api/members - Admin için yeni üye oluşturur.
    [HttpPost("members")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateMember([FromBody] Member model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState });
            }

            // Email kontrolü - LINQ sorgusu ile aynı email var mı kontrol et
            var emailExists = await _context.Members
                .AnyAsync(m => m.Email == model.Email);

            if (emailExists)
            {
                return BadRequest(new { success = false, message = "Bu e-posta adresi zaten kullanılıyor." });
            }

            // Yeni üye oluştur
            model.RegistrationDate = DateTime.UtcNow;
            _context.Members.Add(model);
            await _context.SaveChangesAsync();

            // User tablosuna da ekle (rol bazlı yetkilendirme için)
            var userExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (!userExists)
            {
                var user = new User
                {
                    Email = model.Email,
                    Password = model.Password,
                    Role = "User",
                    CreatedDate = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetMemberById), new { id = model.Id }, new { success = true, data = model });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // PUT: /api/api/members/{id} - Admin için üye günceller.
    [HttpPut("members/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] Member model)
    {
        try
        {
            if (id != model.Id)
            {
                return BadRequest(new { success = false, message = "ID uyuşmazlığı." });
            }

            var existingMember = await _context.Members.FindAsync(id);
            if (existingMember == null)
            {
                return NotFound(new { success = false, message = "Üye bulunamadı." });
            }

            // Email kontrolü - başka bir üyede aynı email var mı?
            var emailExists = await _context.Members
                .AnyAsync(m => m.Email == model.Email && m.Id != id);

            if (emailExists)
            {
                return BadRequest(new { success = false, message = "Bu e-posta adresi başka bir üye tarafından kullanılıyor." });
            }

            // Güncelleme
            existingMember.FirstName = model.FirstName;
            existingMember.LastName = model.LastName;
            existingMember.Email = model.Email;
            existingMember.Phone = model.Phone;
            if (!string.IsNullOrEmpty(model.Password))
            {
                existingMember.Password = model.Password;
            }

            await _context.SaveChangesAsync();

            // User tablosunu da güncelle
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == existingMember.Email);
            if (user != null)
            {
                user.Email = model.Email;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, data = existingMember });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // DELETE: /api/api/members/{id} - Admin için üye siler.
    [HttpDelete("members/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        try
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound(new { success = false, message = "Üye bulunamadı." });
            }

            // User tablosundan da sil
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == member.Email);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // POST: /api/api/members/register - Yeni üye kaydı oluşturur (Public).
    [HttpPost("members/register")]
    public async Task<IActionResult> RegisterMember([FromBody] Member model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState });
            }

            // Email kontrolü - LINQ sorgusu ile aynı email var mı kontrol et
            var emailExists = await _context.Members
                .AnyAsync(m => m.Email == model.Email);

            if (emailExists)
            {
                return BadRequest(new { success = false, message = "Bu e-posta adresi zaten kullanılıyor." });
            }

            // Yeni üye oluştur
            model.RegistrationDate = DateTime.UtcNow;
            _context.Members.Add(model);
            await _context.SaveChangesAsync();

            // User tablosuna da ekle (rol bazlı yetkilendirme için)
            var user = new User
            {
                Email = model.Email,
                Password = model.Password,
                Role = "User",
                CreatedDate = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, data = new { id = model.Id, email = model.Email, firstName = model.FirstName, lastName = model.LastName } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // ==========================================================
    // APPOINTMENT CREATE API ENDPOINT
    // ==========================================================

    // POST: /api/api/appointments - Yeni randevu oluşturur (Sadece User rolü).
    [HttpPost("appointments")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            // Member kontrolü
            var member = await _context.Members.FindAsync(request.MemberId);
            if (member == null)
            {
                return NotFound(new { success = false, message = "Üye bulunamadı." });
            }

            // Trainer kontrolü
            var trainer = await _context.Trainers
                .Include(t => t.GymCenter)
                .FirstOrDefaultAsync(t => t.Id == request.TrainerId);
            if (trainer == null)
            {
                return NotFound(new { success = false, message = "Antrenör bulunamadı." });
            }

            // Activity kontrolü
            var activity = await _context.Activities.FindAsync(request.ActivityId);
            if (activity == null)
            {
                return NotFound(new { success = false, message = "Aktivite bulunamadı." });
            }

            // GymCenter kontrolü
            GymCenter? gymCenter = null;
            if (request.GymCenterId.HasValue)
            {
                gymCenter = await _context.GymCenters.FindAsync(request.GymCenterId.Value);
                if (gymCenter == null)
                {
                    return NotFound(new { success = false, message = "Spor salonu bulunamadı." });
                }
            }
            else if (trainer.GymCenterId > 0)
            {
                gymCenter = await _context.GymCenters.FindAsync(trainer.GymCenterId);
            }

            // Tarih ve saat parse et
            if (!DateTime.TryParse(request.AppointmentDate, out var appointmentDate))
            {
                return BadRequest(new { success = false, message = "Geçersiz tarih formatı." });
            }

            if (!TimeSpan.TryParse(request.AppointmentTime, out var appointmentTime))
            {
                return BadRequest(new { success = false, message = "Geçersiz saat formatı." });
            }

            // Müsaitlik kontrolü - LINQ sorgusu ile aynı tarih/saatte başka randevu var mı?
            var conflictingAppointment = await _context.Appointments
                .Where(a => a.TrainerId == request.TrainerId &&
                           a.AppointmentDate.Date == appointmentDate.Date &&
                           a.AppointmentTime.Hours == appointmentTime.Hours &&
                           a.AppointmentTime.Minutes == appointmentTime.Minutes &&
                           a.Status == AppointmentStatus.Approved)
                .FirstOrDefaultAsync();

            if (conflictingAppointment != null)
            {
                return BadRequest(new { success = false, message = "Bu tarih ve saatte antrenör müsait değil." });
            }

            // Randevu oluştur
            var appointment = new Appointment
            {
                MemberId = request.MemberId,
                TrainerId = request.TrainerId,
                ActivityId = request.ActivityId,
                GymCenterId = gymCenter != null ? gymCenter.Id : trainer.GymCenterId,
                AppointmentDate = appointmentDate,
                AppointmentTime = appointmentTime,
                Price = request.Price ?? activity.Price,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllAppointments), new { id = appointment.Id }, 
                new { success = true, data = new { id = appointment.Id, appointmentDate = appointment.AppointmentDate, appointmentTime = appointment.AppointmentTime.ToString(@"hh\:mm") } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    // Randevu oluşturma için request model
    public class CreateAppointmentRequest
    {
        public int MemberId { get; set; }
        public int TrainerId { get; set; }
        public int ActivityId { get; set; }
        public int? GymCenterId { get; set; }
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
        public decimal? Price { get; set; }
    }
}


