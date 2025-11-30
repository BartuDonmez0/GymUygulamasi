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
        // SIKI KONTROL: Aynı saatte onaylı randevu var mı? (Antrenör için)
        var existsAtSameTimeForTrainer = await ExistsAtSameTimeAsync(
            appointment.TrainerId,
            appointment.AppointmentDate,
            appointment.AppointmentTime);

        if (existsAtSameTimeForTrainer)
        {
            throw new InvalidOperationException("Bu saatte antrenör için zaten onaylanmış bir randevu mevcut. Lütfen başka bir saat seçin.");
        }

        // SIKI KONTROL: Kullanıcının aynı saatte onaylı randevusu var mı?
        var existsAtSameTimeForMember = await ExistsAtSameTimeForMemberAsync(
            appointment.MemberId,
            appointment.AppointmentDate,
            appointment.AppointmentTime);

        if (existsAtSameTimeForMember)
        {
            throw new InvalidOperationException("Bu saatte zaten onaylanmış bir randevunuz bulunmaktadır. Onaylı randevunuzun yanında yeni randevu talebi oluşturamazsınız.");
        }

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
            // Eğer onaylanıyorsa, aynı saatte başka onaylı randevu var mı kontrol et
            if (status == AppointmentStatus.Approved)
            {
                // Mevcut randevunun durumunu kontrol et - eğer zaten onaylıysa, değişiklik yapmaya gerek yok
                if (appointment.Status == AppointmentStatus.Approved)
                {
                    // Zaten onaylı, değişiklik yapmaya gerek yok
                    return;
                }

                // Aynı saatte başka onaylı randevu var mı kontrol et
                var existsAtSameTime = await ExistsAtSameTimeAsync(
                    appointment.TrainerId,
                    appointment.AppointmentDate,
                    appointment.AppointmentTime);

                if (existsAtSameTime)
                {
                    throw new InvalidOperationException("Bu saatte zaten onaylanmış başka bir randevu mevcut. Önce o randevuyu iptal edin veya reddedin.");
                }
            }

            appointment.Status = status;
            await _appointmentRepository.UpdateAsync(appointment);
        }
    }

    public async Task<bool> ExistsAtSameTimeAsync(int trainerId, DateTime appointmentDate, TimeSpan appointmentTime)
    {
        return await _appointmentRepository.ExistsAtSameTimeAsync(trainerId, appointmentDate, appointmentTime);
    }

    public async Task<bool> ExistsAtSameTimeForMemberAsync(int memberId, DateTime appointmentDate, TimeSpan appointmentTime)
    {
        return await _appointmentRepository.ExistsAtSameTimeForMemberAsync(memberId, appointmentDate, appointmentTime);
    }
}

