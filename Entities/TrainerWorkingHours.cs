using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Antrenörlerin haftalık çalışma saatlerini (gün + saat aralığı) temsil eder.
public class TrainerWorkingHours
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Çalışma saatlerinin ait olduğu antrenörün kimliği (FK)
    [Required(ErrorMessage = "Antrenör seçimi zorunludur.")]
    public int TrainerId { get; set; }
    
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
    public Trainer Trainer { get; set; } = null!;
}

