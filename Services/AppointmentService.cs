using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _appointmentRepository.GetWithDetailsAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByMemberIdAsync(int memberId)
    {
        return await _appointmentRepository.GetByMemberIdAsync(memberId);
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _appointmentRepository.GetWithDetailsAsync(id);
    }

    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        return await _appointmentRepository.AddAsync(appointment);
    }

    public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
    {
        await _appointmentRepository.UpdateAsync(appointment);
        return appointment;
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment != null)
        {
            await _appointmentRepository.DeleteAsync(appointment);
        }
    }

    public async Task UpdateAppointmentStatusAsync(int id, AppointmentStatus status)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment != null)
        {
            appointment.Status = status;
            await _appointmentRepository.UpdateAsync(appointment);
        }
    }
}

