using GymApp.Entities;

namespace GymApp.Services;

public interface ITrainerService
{
    Task<IEnumerable<Trainer>> GetAllTrainersAsync();
    Task<Trainer?> GetTrainerByIdAsync(int id);
    Task<Trainer> CreateTrainerAsync(Trainer trainer);
    Task<Trainer> UpdateTrainerAsync(Trainer trainer);
    Task DeleteTrainerAsync(int id);
}

