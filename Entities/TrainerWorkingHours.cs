using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class TrainerWorkingHours
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Antrenör seçimi zorunludur.")]
    public int TrainerId { get; set; }
    
    [Required(ErrorMessage = "Gün zorunludur.")]
    public DayOfWeek DayOfWeek { get; set; }
    
    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    public TimeSpan StartTime { get; set; }
    
    [Required(ErrorMessage = "Bitiş saati zorunludur.")]
    public TimeSpan EndTime { get; set; }
    
    // Navigation property
    public Trainer Trainer { get; set; } = null!;
}

