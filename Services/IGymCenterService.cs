using GymApp.Entities;

namespace GymApp.Services;

public interface IGymCenterService
{
    Task<IEnumerable<GymCenter>> GetAllGymCentersAsync();
    Task<GymCenter?> GetGymCenterByIdAsync(int id);
    Task<GymCenter?> GetGymCenterWithDetailsAsync(int id);
    Task<GymCenter> CreateGymCenterAsync(GymCenter gymCenter);
    Task<GymCenter> UpdateGymCenterAsync(GymCenter gymCenter);
    Task DeleteGymCenterAsync(int id);
    Task<IEnumerable<GymCenter>> GetFilteredGymCentersAsync(int? dayOfWeek, int? activityId);
}

