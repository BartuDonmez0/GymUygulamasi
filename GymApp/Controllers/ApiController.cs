using Microsoft.AspNetCore.Mvc;
using GymApp.Services;
using GymApp.Repositories;
using GymApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers;

/// <summary>
/// REST API Controller - Veritabanı ile iletişim için REST API endpoint'leri sağlar
/// LINQ sorguları ile filtreleme işlemleri bu controller üzerinden gerçekleştirilir
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    private readonly ITrainerService _trainerService;
    private readonly IAppointmentService _appointmentService;
    private readonly IMemberRepository _memberRepository;
    private readonly GymApp.Data.GymAppDbContext _context;

    /// <summary>
    /// API Controller constructor - Dependency injection ile servisleri alır
    /// </summary>
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

    /// <summary>
    /// Tüm antrenörleri listeler - GET /api/api/trainers
    /// LINQ sorgusu ile veritabanından tüm antrenörleri çeker
    /// </summary>
    /// <returns>Antrenör listesi (JSON formatında)</returns>
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

    /// <summary>
    /// Belirli bir tarihte uygun antrenörleri getirir - GET /api/api/available-trainers
    /// LINQ sorgusu ile tarih ve saat bazlı filtreleme yapar
    /// </summary>
    /// <param name="date">Randevu tarihi (format: yyyy-MM-dd)</param>
    /// <param name="time">Randevu saati (format: HH:mm)</param>
    /// <returns>Uygun antrenör listesi (JSON formatında)</returns>
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

    /// <summary>
    /// Üye randevularını getirir - GET /api/api/member-appointments
    /// LINQ sorgusu ile üye ID'sine göre filtreleme yapar
    /// </summary>
    /// <param name="memberId">Üye ID'si</param>
    /// <param name="status">Randevu durumu (opsiyonel: OnayBekliyor, Onaylandi, Reddedildi)</param>
    /// <returns>Üye randevuları listesi (JSON formatında)</returns>
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

    /// <summary>
    /// Antrenörleri uzmanlık alanına göre filtreler - GET /api/api/trainers-by-specialization
    /// LINQ sorgusu ile uzmanlık alanı bazlı filtreleme yapar
    /// </summary>
    /// <param name="specialization">Uzmanlık alanı (örn: "Fitness", "Yoga")</param>
    /// <returns>Filtrelenmiş antrenör listesi (JSON formatında)</returns>
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

    /// <summary>
    /// Spor salonlarını aktiviteye göre filtreler - GET /api/api/gym-centers-by-activity
    /// LINQ sorgusu ile aktivite bazlı filtreleme yapar
    /// </summary>
    /// <param name="activityId">Aktivite ID'si</param>
    /// <returns>Filtrelenmiş spor salonu listesi (JSON formatında)</returns>
    [HttpGet("gym-centers-by-activity")]
    public async Task<IActionResult> GetGymCentersByActivity([FromQuery] int? activityId)
    {
        try
        {
            if (!activityId.HasValue)
            {
                return BadRequest(new { success = false, message = "activityId parametresi gereklidir." });
            }

            // LINQ sorgusu: Belirli aktiviteye sahip spor salonlarını filtrele
            var gymCenters = await _context.GymCenters
                .Include(g => g.Activities)
                .Where(g => g.Activities.Any(a => a.Id == activityId.Value))
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Address,
                    g.Phone,
                    g.Email,
                    g.Description,
                    g.ImageUrl,
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
                activityId = activityId.Value
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
        }
    }
}

