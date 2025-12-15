using GymApp.Entities;

namespace GymApp.Repositories;

// Trainer için özel sorgu ve ilişki imzalarını tanımlar.
public interface ITrainerRepository : IRepository<Trainer>
{
    Task<Trainer?> GetByEmailAsync(string email);
    Task<IEnumerable<Trainer>> GetWithAppointmentsAsync();
    Task<Trainer?> GetWithAppointmentsAsync(int id);
    Task<Trainer?> GetWithWorkingHoursAsync(int id);
    Task<IEnumerable<Trainer>> GetAllWithWorkingHoursAsync();
}

