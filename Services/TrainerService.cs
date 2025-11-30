using GymApp.Entities;
using GymApp.Repositories;
using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class TrainerService : ITrainerService
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly GymAppDbContext _context;

    public TrainerService(ITrainerRepository trainerRepository, GymAppDbContext context)
    {
        _trainerRepository = trainerRepository;
        _context = context;
    }

    public async Task<IEnumerable<Trainer>> GetAllTrainersAsync()
    {
        return await _trainerRepository.GetAllWithWorkingHoursAsync();
    }

    public async Task<Trainer?> GetTrainerByIdAsync(int id)
    {
        return await _trainerRepository.GetWithWorkingHoursAsync(id);
    }

    public async Task<Trainer> CreateTrainerAsync(Trainer trainer)
    {
        // TrainerActivities'i geçici olarak sakla
        var trainerActivities = trainer.TrainerActivities?.ToList() ?? new List<TrainerActivity>();
        trainer.TrainerActivities = new List<TrainerActivity>(); // Navigation property'yi temizle
        
        // Trainer'ı kaydet
        var createdTrainer = await _trainerRepository.AddAsync(trainer);
        
        // TrainerActivities'i kaydet
        if (trainerActivities.Any())
        {
            foreach (var ta in trainerActivities)
            {
                ta.TrainerId = createdTrainer.Id;
                ta.Id = 0; // Yeni kayıt
            }
            await _context.TrainerActivities.AddRangeAsync(trainerActivities);
            await _context.SaveChangesAsync();
        }
        
        return createdTrainer;
    }

    public async Task<Trainer> UpdateTrainerAsync(Trainer trainer)
    {
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
            foreach (var ta in trainerActivities)
            {
                ta.TrainerId = trainer.Id;
                ta.Id = 0; // Yeni kayıt
            }
            await _context.TrainerActivities.AddRangeAsync(trainerActivities);
        }
        
        // TrainerActivities navigation property'yi temizle (güncelleme için)
        trainer.TrainerActivities = new List<TrainerActivity>();
        
        // Trainer'ı güncelle
        await _trainerRepository.UpdateAsync(trainer);
        await _context.SaveChangesAsync();
        
        return trainer;
    }

    public async Task DeleteTrainerAsync(int id)
    {
        var trainer = await _trainerRepository.GetByIdAsync(id);
        if (trainer != null)
        {
            await _trainerRepository.DeleteAsync(trainer);
        }
    }
}

