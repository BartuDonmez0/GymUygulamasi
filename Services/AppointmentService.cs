using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

// Randevu (Appointment) ile ilgili iş kurallarını yöneten servis.
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    // Constructor - randevu repository bağımlılığını alır.
    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    // Tüm randevuları detay bilgileriyle birlikte döndürür.
    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _appointmentRepository.GetWithDetailsAsync();
    }

    // Belirli bir üyeye ait randevuları döndürür.
    public async Task<IEnumerable<Appointment>> GetAppointmentsByMemberIdAsync(int memberId)
    {
        return await _appointmentRepository.GetByMemberIdAsync(memberId);
    }

    // Id'ye göre tek bir randevuyu detaylarıyla birlikte döndürür.
    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _appointmentRepository.GetWithDetailsAsync(id);
    }

    // Yeni randevu oluşturur, saat çakışmalarını kontrol eder.
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

    // Var olan bir randevuyu günceller.
    public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
    {
        await _appointmentRepository.UpdateAsync(appointment);
        return appointment;
    }

    // Id'ye göre randevu kaydını siler (varsa).
    public async Task DeleteAppointmentAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment != null)
        {
            await _appointmentRepository.DeleteAsync(appointment);
        }
    }

    // Randevunun durumunu günceller, onay sırasında çakışma kontrolü yapar.
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

    // Belirli antrenör için aynı tarih ve saatte onaylı randevu olup olmadığını kontrol eder.
    public async Task<bool> ExistsAtSameTimeAsync(int trainerId, DateTime appointmentDate, TimeSpan appointmentTime)
    {
        return await _appointmentRepository.ExistsAtSameTimeAsync(trainerId, appointmentDate, appointmentTime);
    }

    // Belirli üye için aynı tarih ve saatte onaylı randevu olup olmadığını kontrol eder.
    public async Task<bool> ExistsAtSameTimeForMemberAsync(int memberId, DateTime appointmentDate, TimeSpan appointmentTime)
    {
        return await _appointmentRepository.ExistsAtSameTimeForMemberAsync(memberId, appointmentDate, appointmentTime);
    }
}

