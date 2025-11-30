using GymApp.Entities;

namespace GymApp.Repositories;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByMemberIdAsync(int memberId);
    Task<IEnumerable<Appointment>> GetByTrainerIdAsync(int trainerId);
    Task<IEnumerable<Appointment>> GetByGymCenterIdAsync(int gymCenterId);
    Task<IEnumerable<Appointment>> GetWithDetailsAsync();
    Task<Appointment?> GetWithDetailsAsync(int id);
    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status);
    Task<bool> ExistsAtSameTimeAsync(int trainerId, DateTime appointmentDate, TimeSpan appointmentTime);
    Task<bool> ExistsAtSameTimeForMemberAsync(int memberId, DateTime appointmentDate, TimeSpan appointmentTime);
}

