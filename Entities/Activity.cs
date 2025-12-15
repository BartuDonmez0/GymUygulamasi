using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Bu sınıf spor salonunda sunulan her bir aktivite / dersi temsil eder.
public class Activity
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // İlgili spor salonunun kimliği (FK)
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    // Aktivite adı (örn: Fitness, Yoga, Pilates)
    [Required(ErrorMessage = "Aktivite adı zorunludur.")]
    public string Name { get; set; } = string.Empty;
    
    // Detaylı açıklama
    [Required(ErrorMessage = "Açıklama zorunludur.")]
    public string Description { get; set; } = string.Empty;
    
    // Aktivite tipi (enum) – raporlama ve filtreleme için kullanılır
    [Required(ErrorMessage = "Aktivite tipi zorunludur.")]
    public ActivityType Type { get; set; } // Enum: Fitness, Yoga, Pilates, vb.
    
    // Süre (dakika cinsinden)
    [Required(ErrorMessage = "Süre zorunludur.")]
    [Range(1, int.MaxValue, ErrorMessage = "Süre 1 dakikadan fazla olmalıdır.")]
    public int Duration { get; set; } // Dakika cinsinden
    
    // Fiyat bilgisi
    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal Price { get; set; }
    
    // Aktivite görseli (opsiyonel)
    public string ImageUrl { get; set; } = string.Empty;
    
    // İlişkiler
    // Bu aktivitenin ait olduğu spor salonu
    public GymCenter GymCenter { get; set; } = null!;

    // Bu aktiviteyi veren antrenörler (çoktan çoğa ilişki)
    public List<TrainerActivity> TrainerActivities { get; set; } = new();

    // Bu aktivite için alınan randevular
    public List<Appointment> Appointments { get; set; } = new();
}

