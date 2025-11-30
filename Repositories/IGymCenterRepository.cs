using GymApp.Entities;

namespace GymApp.Repositories;

public interface IGymCenterRepository : IRepository<GymCenter>
{
    Task<IEnumerable<GymCenter>> GetWithActivitiesAsync();
    Task<GymCenter?> GetWithActivitiesAsync(int id);
    Task<GymCenter?> GetWithPhotosAsync(int id);
    Task<GymCenter?> GetFullDetailsAsync(int id);
    Task<GymCenter?> GetWithWorkingHoursAsync(int id);
    Task<IEnumerable<GymCenter>> GetAllWithWorkingHoursAsync();
    Task<IEnumerable<GymCenter>> GetFilteredAsync(int? dayOfWeek, int? activityId);
}

