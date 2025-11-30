using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class Appointment
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Üye seçimi zorunludur.")]
    public int MemberId { get; set; }
    
    [Required(ErrorMessage = "Antrenör seçimi zorunludur.")]
    public int TrainerId { get; set; }
    
    [Required(ErrorMessage = "Aktivite seçimi zorunludur.")]
    public int ActivityId { get; set; }
    
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
    public DateTime AppointmentDate { get; set; }
    
    [Required(ErrorMessage = "Randevu saati zorunludur.")]
    public TimeSpan AppointmentTime { get; set; }
    
    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "Durum zorunludur.")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending; // Pending, Approved, Rejected, Completed
    
    // Navigation properties
    public Member Member { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public Activity Activity { get; set; } = null!;
    public GymCenter GymCenter { get; set; } = null!;
}

