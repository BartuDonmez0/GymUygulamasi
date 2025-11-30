using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;

    public ActivityService(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
    {
        return await _activityRepository.GetWithGymCenterAsync();
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByGymCenterIdAsync(int gymCenterId)
    {
        return await _activityRepository.GetByGymCenterIdAsync(gymCenterId);
    }

    public async Task<Activity?> GetActivityByIdAsync(int id)
    {
        return await _activityRepository.GetWithGymCenterAsync(id);
    }

    public async Task<Activity> CreateActivityAsync(Activity activity)
    {
        return await _activityRepository.AddAsync(activity);
    }

    public async Task<Activity> UpdateActivityAsync(Activity activity)
    {
        await _activityRepository.UpdateAsync(activity);
        return activity;
    }

    public async Task DeleteActivityAsync(int id)
    {
        var activity = await _activityRepository.GetByIdAsync(id);
        if (activity != null)
        {
            await _activityRepository.DeleteAsync(activity);
        }
    }
}

