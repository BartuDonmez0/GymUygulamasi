using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class Trainer
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
    
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
    public string Specialization { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;
    
    // Çalışma saatleri JSON formatında
    public string WorkingHoursJson { get; set; } = "[]"; // JSON array: [{"Day":1,"TimeRange":"09:00-18:00"},...]
    
    // Navigation properties
    public GymCenter GymCenter { get; set; } = null!;
    public List<Appointment> Appointments { get; set; } = new();
    public List<TrainerActivity> TrainerActivities { get; set; } = new();
    public List<TrainerWorkingHours> WorkingHours { get; set; } = new(); // Eski format (backward compatibility)
}

