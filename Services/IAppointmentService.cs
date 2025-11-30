using GymApp.Entities;

namespace GymApp.Services;

public interface IAppointmentService
{
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<IEnumerable<Appointment>> GetAppointmentsByMemberIdAsync(int memberId);
    Task<Appointment?> GetAppointmentByIdAsync(int id);
    Task<Appointment> CreateAppointmentAsync(Appointment appointment);
    Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
    Task DeleteAppointmentAsync(int id);
    Task UpdateAppointmentStatusAsync(int id, AppointmentStatus status);
    Task<bool> ExistsAtSameTimeAsync(int trainerId, DateTime appointmentDate, TimeSpan appointmentTime);
    Task<bool> ExistsAtSameTimeForMemberAsync(int memberId, DateTime appointmentDate, TimeSpan appointmentTime);
}

