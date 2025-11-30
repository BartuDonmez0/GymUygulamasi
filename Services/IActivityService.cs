using GymApp.Entities;

namespace GymApp.Services;

public interface IActivityService
{
    Task<IEnumerable<Activity>> GetAllActivitiesAsync();
    Task<IEnumerable<Activity>> GetActivitiesByGymCenterIdAsync(int gymCenterId);
    Task<Activity?> GetActivityByIdAsync(int id);
    Task<Activity> CreateActivityAsync(Activity activity);
    Task<Activity> UpdateActivityAsync(Activity activity);
    Task DeleteActivityAsync(int id);
}

