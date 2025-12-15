using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Spor salonlarının haftalık çalışma saatlerini (gün + saat aralığı) temsil eder.
public class GymCenterWorkingHours
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Çalışma saatlerinin ait olduğu spor salonunun kimliği (FK)
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    // Haftanın günü
    [Required(ErrorMessage = "Gün zorunludur.")]
    public DayOfWeek DayOfWeek { get; set; }
    
    // Başlangıç saati
    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    public TimeSpan StartTime { get; set; }
    
    // Bitiş saati
    [Required(ErrorMessage = "Bitiş saati zorunludur.")]
    public TimeSpan EndTime { get; set; }
    
    // İlişkiler
    public GymCenter GymCenter { get; set; } = null!;
}

