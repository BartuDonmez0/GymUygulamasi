using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Bu sınıf sistemdeki spor salonu kayıtlarını temsil eder.
public class GymCenter
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Salon adı
    [Required(ErrorMessage = "Spor salonu adı zorunludur.")]
    public string Name { get; set; } = string.Empty;
    
    // Kısa açıklama / tanıtım metni
    public string Description { get; set; } = string.Empty;
    
    // Salon adresi
    [Required(ErrorMessage = "Adres zorunludur.")]
    public string Address { get; set; } = string.Empty;
    
    // İletişim telefonu
    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string Phone { get; set; } = string.Empty;
    
    // İletişim e‑postası
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;
    
    // Eski metin tabanlı çalışma saatleri alanı (geri uyumluluk için)
    public string WorkingHours { get; set; } = string.Empty;
    
    // Çalışma saatleri JSON formatında tutulur (gün + saat aralığı listesi)
    public string WorkingHoursJson { get; set; } = "[]"; // Örnek: [{"Day":1,"TimeRange":"09:00-18:00"},...]
    
    // Ana sayfada gösterilebilecek reklam / kampanya metni
    public string Advertisement { get; set; } = string.Empty;
    
    // Salonun kapak görseli
    public string ImageUrl { get; set; } = string.Empty;
    
    // Salon aktif mi (listeleme ve rezervasyonlarda kullanılacak)
    public bool IsActive { get; set; } = false;
    
    // Bu salonda sunulan aktiviteler (1‑N ilişki)
    public List<Activity> Activities { get; set; } = new();

    // Salona ait ek fotoğraflar
    public List<GymCenterPhoto> Photos { get; set; } = new();

    // Bu salonda çalışan antrenörler
    public List<Trainer> Trainers { get; set; } = new();

    // Eski tip çalışma saati kayıtları (tablo üzerinden)
    public List<GymCenterWorkingHours> WorkingHoursList { get; set; } = new();
}

