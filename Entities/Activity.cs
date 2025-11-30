using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class Activity
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    [Required(ErrorMessage = "Aktivite adı zorunludur.")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Açıklama zorunludur.")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Aktivite tipi zorunludur.")]
    public ActivityType Type { get; set; } // Enum: Fitness, Yoga, Pilates, vb.
    
    [Required(ErrorMessage = "Süre zorunludur.")]
    [Range(1, int.MaxValue, ErrorMessage = "Süre 1 dakikadan fazla olmalıdır.")]
    public int Duration { get; set; } // Dakika cinsinden
    
    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal Price { get; set; }
    
    public string ImageUrl { get; set; } = string.Empty;
    
    // Navigation properties
    public GymCenter GymCenter { get; set; } = null!;
    public List<TrainerActivity> TrainerActivities { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
}

