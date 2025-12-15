using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Bu sınıf sisteme kayıt olan üyeleri temsil eder.
public class Member
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Üyenin adı
    [Required(ErrorMessage = "Ad zorunludur.")]
    public string FirstName { get; set; } = string.Empty;
    
    // Üyenin soyadı
    [Required(ErrorMessage = "Soyad zorunludur.")]
    public string LastName { get; set; } = string.Empty;
    
    // Üyenin e‑posta adresi (giriş ve iletişim için)
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;
    
    // Üyenin telefon numarası
    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string Phone { get; set; } = string.Empty;
    
    // Giriş için kullanılan şifre
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;
    
    // Üyenin sisteme kayıt olduğu tarih
    [Required(ErrorMessage = "Kayıt tarihi zorunludur.")]
    public DateTime RegistrationDate { get; set; }
    
    // Kimlik ve rol bilgisinin tutulduğu User kaydının kimliği (opsiyonel)
    public int? UserId { get; set; } // User tablosu ile ilişki
    
    // İlişkiler
    // Üyenin User kaydı
    public User? User { get; set; }
    // Üyenin randevuları
    public List<Appointment> Appointments { get; set; } = new();
    // Üyeye üretilen yapay zeka önerileri
    public List<AIRecommendation> Recommendations { get; set; } = new();
    // Üyenin yapay zeka ile yaptığı sohbet mesajları
    public List<ChatMessage> ChatMessages { get; set; } = new();
}

