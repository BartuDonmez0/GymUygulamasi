using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class GymCenter
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Spor salonu adı zorunludur.")]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Adres zorunludur.")]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;
    
    public string WorkingHours { get; set; } = string.Empty; // Eski format için (backward compatibility)
    
    // Çalışma saatleri JSON formatında
    public string WorkingHoursJson { get; set; } = "[]"; // JSON array: [{"Day":1,"TimeRange":"09:00-18:00"},...]
    
    public string Advertisement { get; set; } = string.Empty; // Reklam metni
    
    public string ImageUrl { get; set; } = string.Empty; // Ana görsel
    
    public bool IsActive { get; set; } = false; // Aktiviteler olmadan açık olmamalı
    
    // Navigation properties
    public List<Activity> Activities { get; set; } = new(); // Aktiviteler
    public List<GymCenterPhoto> Photos { get; set; } = new(); // Spor fotoğrafları
    public List<Trainer> Trainers { get; set; } = new(); // Antrenörler
    public List<GymCenterWorkingHours> WorkingHoursList { get; set; } = new(); // Eski format (backward compatibility)
}

