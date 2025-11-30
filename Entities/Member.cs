using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class Member
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Ad zorunludur.")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Soyad zorunludur.")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Kayıt tarihi zorunludur.")]
    public DateTime RegistrationDate { get; set; }
    
    public int? UserId { get; set; } // User entity ile ilişki
    
    // Navigation properties
    public User? User { get; set; }
    public List<Appointment> Appointments { get; set; } = new();
    public List<AIRecommendation> Recommendations { get; set; } = new();
    public List<ChatMessage> ChatMessages { get; set; } = new();
}

