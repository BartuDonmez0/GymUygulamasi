using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class GymCenterWorkingHours
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    [Required(ErrorMessage = "Gün zorunludur.")]
    public DayOfWeek DayOfWeek { get; set; }
    
    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    public TimeSpan StartTime { get; set; }
    
    [Required(ErrorMessage = "Bitiş saati zorunludur.")]
    public TimeSpan EndTime { get; set; }
    
    // Navigation property
    public GymCenter GymCenter { get; set; } = null!;
}

