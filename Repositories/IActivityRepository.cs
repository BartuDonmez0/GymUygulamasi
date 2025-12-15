using GymApp.Entities;

namespace GymApp.Repositories;

// Activity için özel sorgu imzalarını tanımlar.
public interface IActivityRepository : IRepository<Activity>
{
    Task<IEnumerable<Activity>> GetByGymCenterIdAsync(int gymCenterId);
    Task<IEnumerable<Activity>> GetWithGymCenterAsync();
    Task<Activity?> GetWithGymCenterAsync(int id);
}

