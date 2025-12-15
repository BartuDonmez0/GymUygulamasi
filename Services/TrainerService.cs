using GymApp.Entities;
using GymApp.Repositories;
using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

// Antrenör (Trainer) ile ilgili iş kurallarını yöneten servis.
public class TrainerService : ITrainerService
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly GymAppDbContext _context;

    // Constructor - repository ve DbContext bağımlılıklarını alır.
    public TrainerService(ITrainerRepository trainerRepository, GymAppDbContext context)
    {
        _trainerRepository = trainerRepository;
        _context = context;
    }

    // Tüm antrenörleri çalışma saatleriyle birlikte döndürür.
    public async Task<IEnumerable<Trainer>> GetAllTrainersAsync()
    {
        return await _trainerRepository.GetAllWithWorkingHoursAsync();
    }

    // Id'ye göre tek bir antrenörü çalışma saatleriyle birlikte döndürür.
    public async Task<Trainer?> GetTrainerByIdAsync(int id)
    {
        return await _trainerRepository.GetWithWorkingHoursAsync(id);
    }

    // Yeni antrenör kaydı oluşturur, GymCenter ve aktiviteler için doğrulama yapar.
    public async Task<Trainer> CreateTrainerAsync(Trainer trainer)
    {
        // GymCenterId'nin geçerli olup olmadığını kontrol et
        var gymCenterExists = await _context.Set<GymCenter>()
            .AnyAsync(gc => gc.Id == trainer.GymCenterId);
        
        if (!gymCenterExists)
        {
            throw new InvalidOperationException($"Geçersiz GymCenterId: {trainer.GymCenterId}");
        }
        
        // TrainerActivities'i geçici olarak sakla
        var trainerActivities = trainer.TrainerActivities?.ToList() ?? new List<TrainerActivity>();
        trainer.TrainerActivities = new List<TrainerActivity>(); // Navigation property'yi temizle
        
        // WorkingHours navigation property'yi temizle (JSON kullanıyoruz)
        trainer.WorkingHours = new List<TrainerWorkingHours>();
        
        // GymCenter navigation property'yi temizle
        trainer.GymCenter = null!;
        
        // WorkingHoursJson'u doğrula ve düzelt
        if (string.IsNullOrWhiteSpace(trainer.WorkingHoursJson))
        {
            trainer.WorkingHoursJson = "[]";
        }
        else
        {
            // JSON formatını kontrol et
            try
            {
                System.Text.Json.JsonSerializer.Deserialize<object>(trainer.WorkingHoursJson);
            }
            catch
            {
                // Geçersiz JSON ise boş array yap
                trainer.WorkingHoursJson = "[]";
            }
        }
        
        // Trainer'ı kaydet
        var createdTrainer = await _trainerRepository.AddAsync(trainer);
        
        // TrainerActivities'i kaydet (navigation property'leri null yap)
        if (trainerActivities.Any())
        {
            // Geçerli ActivityId'leri kontrol et
            var validActivityIds = await _context.Set<Activity>()
                .Where(a => trainerActivities.Select(ta => ta.ActivityId).Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();
            
            // Duplicate'leri temizle - aynı ActivityId'den sadece bir tane olsun
            var uniqueActivityIds = trainerActivities
                .Select(ta => ta.ActivityId)
                .Distinct()
                .Where(id => validActivityIds.Contains(id))
                .ToList();
            
            // Mevcut TrainerActivities'i kontrol et (duplicate önleme)
            var existingTrainerActivities = await _context.TrainerActivities
                .Where(ta => ta.TrainerId == createdTrainer.Id && 
                            uniqueActivityIds.Contains(ta.ActivityId))
                .Select(ta => ta.ActivityId)
                .ToListAsync();
            
            // Sadece yeni olanları ekle
            var newActivityIds = uniqueActivityIds
                .Where(id => !existingTrainerActivities.Contains(id))
                .ToList();
            
            var validTrainerActivities = newActivityIds.Select(activityId => new TrainerActivity
            {
                TrainerId = createdTrainer.Id,
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
        
        return createdTrainer;
    }

    // Var olan antrenör kaydını, çalışma saatlerini ve aktivitelerini günceller.
    public async Task<Trainer> UpdateTrainerAsync(Trainer trainer)
    {
        // Mevcut trainer'ı veritabanından çek (tracking için)
        var existingTrainer = await _context.Trainers
            .FirstOrDefaultAsync(t => t.Id == trainer.Id);
        
        if (existingTrainer == null)
        {
            throw new InvalidOperationException($"Trainer with Id {trainer.Id} not found.");
        }
        
        // Mevcut çalışma saatlerini sil
        var existingWorkingHours = await _context.TrainerWorkingHours
            .Where(wh => wh.TrainerId == trainer.Id)
            .ToListAsync();
        
        _context.TrainerWorkingHours.RemoveRange(existingWorkingHours);
        
        // Yeni çalışma saatlerini ekle
        if (trainer.WorkingHours != null && trainer.WorkingHours.Any())
        {
            foreach (var workingHour in trainer.WorkingHours)
            {
                workingHour.TrainerId = trainer.Id;
                workingHour.Id = 0; // Yeni kayıt olarak işaretle
            }
            await _context.TrainerWorkingHours.AddRangeAsync(trainer.WorkingHours);
        }
        
        // Mevcut TrainerActivities'i sil
        var existingTrainerActivities = await _context.TrainerActivities
            .Where(ta => ta.TrainerId == trainer.Id)
            .ToListAsync();
        
        _context.TrainerActivities.RemoveRange(existingTrainerActivities);
        
        // Yeni TrainerActivities'i ekle
        var trainerActivities = trainer.TrainerActivities?.ToList() ?? new List<TrainerActivity>();
        if (trainerActivities.Any())
        {
            // Geçerli ActivityId'leri kontrol et
            var validActivityIds = await _context.Set<Activity>()
                .Where(a => trainerActivities.Select(ta => ta.ActivityId).Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();
            
            // Duplicate'leri temizle - aynı ActivityId'den sadece bir tane olsun
            var uniqueActivityIds = trainerActivities
                .Select(ta => ta.ActivityId)
                .Distinct()
                .Where(id => validActivityIds.Contains(id))
                .ToList();
            
            var validTrainerActivities = uniqueActivityIds.Select(activityId => new TrainerActivity
            {
                TrainerId = trainer.Id,
                ActivityId = activityId,
                Trainer = null!,
                Activity = null!
            }).ToList();
            
            if (validTrainerActivities.Any())
            {
                await _context.TrainerActivities.AddRangeAsync(validTrainerActivities);
            }
        }
        
        // WorkingHoursJson'u doğrula ve düzelt
        if (string.IsNullOrWhiteSpace(trainer.WorkingHoursJson))
        {
            trainer.WorkingHoursJson = "[]";
        }
        else
        {
            // JSON formatını kontrol et
            try
            {
                System.Text.Json.JsonSerializer.Deserialize<object>(trainer.WorkingHoursJson);
            }
            catch
            {
                // Geçersiz JSON ise boş array yap
                trainer.WorkingHoursJson = "[]";
            }
        }
        
        // Trainer property'lerini güncelle (navigation property'leri hariç)
        existingTrainer.FirstName = trainer.FirstName;
        existingTrainer.LastName = trainer.LastName;
        existingTrainer.Email = trainer.Email;
        existingTrainer.Phone = trainer.Phone;
        existingTrainer.GymCenterId = trainer.GymCenterId;
        existingTrainer.Specialization = trainer.Specialization;
        existingTrainer.WorkingHoursJson = trainer.WorkingHoursJson;
        existingTrainer.ProfilePhotoUrl = trainer.ProfilePhotoUrl;
        
        // Şifre değiştirilmişse güncelle
        if (!string.IsNullOrEmpty(trainer.Password))
        {
            existingTrainer.Password = trainer.Password;
        }
        
        // Değişiklikleri kaydet
        await _context.SaveChangesAsync();
        
        return existingTrainer;
    }

    // Id'ye göre antrenör kaydını siler (varsa).
    public async Task DeleteTrainerAsync(int id)
    {
        var trainer = await _trainerRepository.GetByIdAsync(id);
        if (trainer != null)
        {
            await _trainerRepository.DeleteAsync(trainer);
        }
    }
}

