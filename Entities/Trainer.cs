using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Bu sınıf sistemdeki antrenör kayıtlarını temsil eder.
public class Trainer
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Antrenörün adı
    [Required(ErrorMessage = "Ad zorunludur.")]
    public string FirstName { get; set; } = string.Empty;
    
    // Antrenörün soyadı
    [Required(ErrorMessage = "Soyad zorunludur.")]
    public string LastName { get; set; } = string.Empty;
    
    // İletişim / giriş e‑postası
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;
    
    // Antrenörün telefon numarası
    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string Phone { get; set; } = string.Empty;
    
    // Çalıştığı spor salonunun kimliği (FK)
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    // Uzmanlık alanları (örn: Fitness, Yoga)
    [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
    public string Specialization { get; set; } = string.Empty;
    
    // Giriş için kullanılan şifre
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;
    
    // Çalışma saatleri JSON formatında
    public string WorkingHoursJson { get; set; } = "[]"; // JSON array: [{"Day":1,"TimeRange":"09:00-18:00"},...]
    
    // Profil fotoğrafı URL
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    
    // İlişkiler
    // Antrenörün bağlı olduğu spor salonu
    public GymCenter GymCenter { get; set; } = null!;
    // Antrenörün aldığı randevular
    public List<Appointment> Appointments { get; set; } = new();
    // Antrenörün verdiği aktiviteler (çoktan çoğa ilişki)
    public List<TrainerActivity> TrainerActivities { get; set; } = new();
    // Eski tip çalışma saati kayıtları
    public List<TrainerWorkingHours> WorkingHours { get; set; } = new(); // Eski format (backward compatibility)
}

