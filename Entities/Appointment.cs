using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Üyelerin antrenör ve aktiviteler için aldıkları randevuları temsil eder.
public class Appointment
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Randevu alan üyenin kimliği (FK)
    [Required(ErrorMessage = "Üye seçimi zorunludur.")]
    public int MemberId { get; set; }
    
    // Randevunun bağlı olduğu antrenörün kimliği (FK)
    [Required(ErrorMessage = "Antrenör seçimi zorunludur.")]
    public int TrainerId { get; set; }
    
    // Randevu alınan aktivitenin kimliği (FK)
    [Required(ErrorMessage = "Aktivite seçimi zorunludur.")]
    public int ActivityId { get; set; }
    
    // Randevunun gerçekleşeceği spor salonunun kimliği (FK)
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    // Randevu tarihi
    [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
    public DateTime AppointmentDate { get; set; }
    
    // Randevu saati
    [Required(ErrorMessage = "Randevu saati zorunludur.")]
    public TimeSpan AppointmentTime { get; set; }
    
    // Toplam fiyat
    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal Price { get; set; }
    
    // Randevunun durumu (beklemede, onaylı vb.)
    [Required(ErrorMessage = "Durum zorunludur.")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending; // Pending, Approved, Rejected, Completed
    
    // İlişkiler
    public Member Member { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public Activity Activity { get; set; } = null!;
    public GymCenter GymCenter { get; set; } = null!;
}

