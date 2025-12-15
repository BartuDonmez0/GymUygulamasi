using Microsoft.EntityFrameworkCore;
using GymApp.Entities;

namespace GymApp.Data;

// Uygulamanın tüm entity'leri ve veritabanı tabloları için EF Core DbContext sınıfı.
public class GymAppDbContext : DbContext
{
    public GymAppDbContext(DbContextOptions<GymAppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<GymCenter> GymCenters { get; set; }
    public DbSet<GymCenterPhoto> GymCenterPhotos { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<TrainerActivity> TrainerActivities { get; set; }
    public DbSet<TrainerWorkingHours> TrainerWorkingHours { get; set; }
    public DbSet<GymCenterWorkingHours> GymCenterWorkingHours { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AIRecommendation> AIRecommendations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Email).HasColumnName("email");
            entity.Property(u => u.Password).HasColumnName("password");
            entity.Property(u => u.Role).HasColumnName("role");
            entity.Property(u => u.CreatedDate)
                .HasColumnName("created_date")
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Member
        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("members");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).HasColumnName("id");
            entity.Property(m => m.FirstName).HasColumnName("first_name");
            entity.Property(m => m.LastName).HasColumnName("last_name");
            entity.Property(m => m.Email).HasColumnName("email");
            entity.Property(m => m.Phone).HasColumnName("phone");
            entity.Property(m => m.Password).HasColumnName("password");
            entity.Property(m => m.RegistrationDate)
                .HasColumnName("registration_date")
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(m => m.UserId).HasColumnName("user_id");
            entity.HasOne(m => m.User)
                .WithOne(u => u.Member)
                .HasForeignKey<Member>(m => m.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(m => m.Email).IsUnique();
        });

        // Trainer
        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.ToTable("trainers");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.FirstName).HasColumnName("first_name");
            entity.Property(t => t.LastName).HasColumnName("last_name");
            entity.Property(t => t.Email).HasColumnName("email");
            entity.Property(t => t.Phone).HasColumnName("phone");
            entity.Property(t => t.Specialization).HasColumnName("specialization");
            entity.Property(t => t.Password).HasColumnName("password");
            entity.Property(t => t.GymCenterId).HasColumnName("gym_center_id");
            entity.Property(t => t.WorkingHoursJson).HasColumnName("working_hours_json").HasDefaultValue("[]");
            entity.Property(t => t.ProfilePhotoUrl).HasColumnName("profile_photo_url").HasDefaultValue("");
            entity.HasOne(t => t.GymCenter)
                .WithMany(g => g.Trainers)
                .HasForeignKey(t => t.GymCenterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(t => t.Email).IsUnique();
        });

        // GymCenter
        modelBuilder.Entity<GymCenter>(entity =>
        {
            entity.ToTable("gym_centers");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Id).HasColumnName("id");
            entity.Property(g => g.Name).HasColumnName("name");
            entity.Property(g => g.Description).HasColumnName("description");
            entity.Property(g => g.Address).HasColumnName("address");
            entity.Property(g => g.Phone).HasColumnName("phone");
            entity.Property(g => g.Email).HasColumnName("email");
            entity.Property(g => g.WorkingHours).HasColumnName("working_hours");
            entity.Property(g => g.WorkingHoursJson).HasColumnName("working_hours_json").HasDefaultValue("[]");
            entity.Property(g => g.Advertisement).HasColumnName("advertisement");
            entity.Property(g => g.ImageUrl).HasColumnName("image_url");
            entity.Property(g => g.IsActive).HasColumnName("is_active").HasDefaultValue(false);
        });

        // GymCenterWorkingHours
        modelBuilder.Entity<GymCenterWorkingHours>(entity =>
        {
            entity.ToTable("gym_center_working_hours");
            entity.HasKey(gwh => gwh.Id);
            entity.Property(gwh => gwh.Id).HasColumnName("id");
            entity.Property(gwh => gwh.GymCenterId).HasColumnName("gym_center_id");
            entity.Property(gwh => gwh.DayOfWeek)
                .HasColumnName("day_of_week")
                .HasConversion<int>(); // Enum'u int olarak sakla
            entity.Property(gwh => gwh.StartTime).HasColumnName("start_time");
            entity.Property(gwh => gwh.EndTime).HasColumnName("end_time");
            entity.HasOne(gwh => gwh.GymCenter)
                .WithMany(g => g.WorkingHoursList)
                .HasForeignKey(gwh => gwh.GymCenterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GymCenterPhoto
        modelBuilder.Entity<GymCenterPhoto>(entity =>
        {
            entity.ToTable("gym_center_photos");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.GymCenterId).HasColumnName("gym_center_id");
            entity.Property(p => p.PhotoUrl).HasColumnName("photo_url");
            entity.HasOne(p => p.GymCenter)
                .WithMany(g => g.Photos)
                .HasForeignKey(p => p.GymCenterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Activity
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("activities");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.GymCenterId).HasColumnName("gym_center_id");
            entity.Property(a => a.Name).HasColumnName("name");
            entity.Property(a => a.Description).HasColumnName("description");
            entity.Property(a => a.Type)
                .HasColumnName("type")
                .HasConversion<int>(); // Enum'u int olarak sakla
            entity.Property(a => a.Duration).HasColumnName("duration");
            entity.Property(a => a.Price).HasColumnName("price");
            entity.Property(a => a.ImageUrl).HasColumnName("image_url");
            entity.HasOne(a => a.GymCenter)
                .WithMany(g => g.Activities)
                .HasForeignKey(a => a.GymCenterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TrainerActivity (Many-to-Many)
        modelBuilder.Entity<TrainerActivity>(entity =>
        {
            entity.ToTable("trainer_activities");
            entity.HasKey(ta => ta.Id);
            entity.Property(ta => ta.Id).HasColumnName("id");
            entity.Property(ta => ta.TrainerId).HasColumnName("trainer_id");
            entity.Property(ta => ta.ActivityId).HasColumnName("activity_id");
            entity.HasOne(ta => ta.Trainer)
                .WithMany(t => t.TrainerActivities)
                .HasForeignKey(ta => ta.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ta => ta.Activity)
                .WithMany(a => a.TrainerActivities)
                .HasForeignKey(ta => ta.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(ta => new { ta.TrainerId, ta.ActivityId }).IsUnique();
        });

        // TrainerWorkingHours
        modelBuilder.Entity<TrainerWorkingHours>(entity =>
        {
            entity.ToTable("trainer_working_hours");
            entity.HasKey(twh => twh.Id);
            entity.Property(twh => twh.Id).HasColumnName("id");
            entity.Property(twh => twh.TrainerId).HasColumnName("trainer_id");
            entity.Property(twh => twh.DayOfWeek)
                .HasColumnName("day_of_week")
                .HasConversion<int>(); // Enum'u int olarak sakla
            entity.Property(twh => twh.StartTime).HasColumnName("start_time");
            entity.Property(twh => twh.EndTime).HasColumnName("end_time");
            entity.HasOne(twh => twh.Trainer)
                .WithMany(t => t.WorkingHours)
                .HasForeignKey(twh => twh.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.MemberId).HasColumnName("member_id");
            entity.Property(a => a.TrainerId).HasColumnName("trainer_id");
            entity.Property(a => a.ActivityId).HasColumnName("activity_id");
            entity.Property(a => a.GymCenterId).HasColumnName("gym_center_id");
            entity.Property(a => a.AppointmentDate)
                .HasColumnName("appointment_date")
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(a => a.AppointmentTime).HasColumnName("appointment_time");
            entity.Property(a => a.Price).HasColumnName("price");
            entity.Property(a => a.Status)
                .HasColumnName("status")
                .HasConversion<int>(); // Enum'u int olarak sakla
            entity.HasOne(a => a.Member)
                .WithMany(m => m.Appointments)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Activity)
                .WithMany(act => act.Appointments)
                .HasForeignKey(a => a.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.GymCenter)
                .WithMany()
                .HasForeignKey(a => a.GymCenterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AIRecommendation
        modelBuilder.Entity<AIRecommendation>(entity =>
        {
            entity.ToTable("ai_recommendations");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id");
            entity.Property(r => r.MemberId).HasColumnName("member_id");
            entity.Property(r => r.RecommendationType).HasColumnName("recommendation_type");
            entity.Property(r => r.Content).HasColumnName("content");
            entity.Property(r => r.CreatedDate)
                .HasColumnName("created_date")
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.HasOne(r => r.Member)
                .WithMany(m => m.Recommendations)
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChatMessage
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(cm => cm.Id);
            entity.Property(cm => cm.Id).HasColumnName("id");
            entity.Property(cm => cm.MemberId).HasColumnName("member_id");
            entity.Property(cm => cm.Message).HasColumnName("message");
            entity.Property(cm => cm.Response).HasColumnName("response");
            entity.Property(cm => cm.CreatedDate)
                .HasColumnName("created_date")
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.HasOne(cm => cm.Member)
                .WithMany(m => m.ChatMessages)
                .HasForeignKey(cm => cm.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
