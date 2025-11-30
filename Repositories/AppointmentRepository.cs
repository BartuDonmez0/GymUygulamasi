using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(GymAppDbContext context) : base(context)
    {
    }

    // Override AddAsync to add database-level validation
    public override async Task<Appointment> AddAsync(Appointment entity)
    {
        // Transaction içinde double-check yap
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // VERİTABANI SEVİYESİNDE KONTROL: Aynı saatte onaylı randevu var mı?
            // TimeSpan karşılaştırması için sadece saat ve dakikayı al
            var normalizedTime = new TimeSpan(entity.AppointmentTime.Hours, entity.AppointmentTime.Minutes, 0);
            
            var existsAtSameTimeForTrainer = await _dbSet
                .AnyAsync(a => a.TrainerId == entity.TrainerId &&
                              a.AppointmentDate.Date == entity.AppointmentDate.Date &&
                              a.AppointmentTime.Hours == normalizedTime.Hours &&
                              a.AppointmentTime.Minutes == normalizedTime.Minutes &&
                              a.Status == AppointmentStatus.Approved);

            if (existsAtSameTimeForTrainer)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Bu saatte antrenör için zaten onaylanmış bir randevu mevcut. Lütfen başka bir saat seçin.");
            }

            // VERİTABANI SEVİYESİNDE KONTROL: Kullanıcının aynı saatte onaylı randevusu var mı?
            var existsAtSameTimeForMember = await _dbSet
                .AnyAsync(a => a.MemberId == entity.MemberId &&
                              a.AppointmentDate.Date == entity.AppointmentDate.Date &&
                              a.AppointmentTime.Hours == normalizedTime.Hours &&
                              a.AppointmentTime.Minutes == normalizedTime.Minutes &&
                              a.Status == AppointmentStatus.Approved);

            if (existsAtSameTimeForMember)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Bu saatte zaten onaylanmış bir randevunuz bulunmaktadır. Onaylı randevunuzun yanında yeni randevu talebi oluşturamazsınız.");
            }

            // Kontrol başarılı, randevuyu ekle
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return entity;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<Appointment>> GetByMemberIdAsync(int memberId)
    {
        return await _dbSet
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Activity)
            .Include(a => a.GymCenter)
            .Where(a => a.MemberId == memberId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByTrainerIdAsync(int trainerId)
    {
        return await _dbSet
            .Where(a => a.TrainerId == trainerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByGymCenterIdAsync(int gymCenterId)
    {
        return await _dbSet
            .Where(a => a.GymCenterId == gymCenterId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetWithDetailsAsync()
    {
        return await _dbSet
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Activity)
            .Include(a => a.GymCenter)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Activity)
            .Include(a => a.GymCenter)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status)
    {
        return await _dbSet
            .Where(a => a.Status == status)
            .ToListAsync();
    }

    public async Task<bool> ExistsAtSameTimeAsync(int trainerId, DateTime appointmentDate, TimeSpan appointmentTime)
    {
        // TimeSpan karşılaştırması için sadece saat ve dakikayı al (saniye ve milisaniye yok say)
        var normalizedTime = new TimeSpan(appointmentTime.Hours, appointmentTime.Minutes, 0);
        
        return await _dbSet
            .AnyAsync(a => a.TrainerId == trainerId &&
                          a.AppointmentDate.Date == appointmentDate.Date &&
                          a.AppointmentTime.Hours == normalizedTime.Hours &&
                          a.AppointmentTime.Minutes == normalizedTime.Minutes &&
                          a.Status == AppointmentStatus.Approved);
    }

    public async Task<bool> ExistsAtSameTimeForMemberAsync(int memberId, DateTime appointmentDate, TimeSpan appointmentTime)
    {
        // TimeSpan karşılaştırması için sadece saat ve dakikayı al (saniye ve milisaniye yok say)
        var normalizedTime = new TimeSpan(appointmentTime.Hours, appointmentTime.Minutes, 0);
        
        return await _dbSet
            .AnyAsync(a => a.MemberId == memberId &&
                          a.AppointmentDate.Date == appointmentDate.Date &&
                          a.AppointmentTime.Hours == normalizedTime.Hours &&
                          a.AppointmentTime.Minutes == normalizedTime.Minutes &&
                          a.Status == AppointmentStatus.Approved);
    }
}

