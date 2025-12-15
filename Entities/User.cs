using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

// Kimlik doğrulama ve rol (Admin / User) bilgilerini temsil eder.
public class User
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Oturum açma ve iletişim için kullanılan e‑posta
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;
    
    // Giriş için kullanılan şifre
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;
    
    // Kullanıcının rolü (Admin veya Member)
    [Required(ErrorMessage = "Rol zorunludur.")]
    public string Role { get; set; } = "Member"; // Admin veya Member
    
    // Kullanıcının oluşturulduğu tarih
    [Required(ErrorMessage = "Oluşturulma tarihi zorunludur.")]
    public DateTime CreatedDate { get; set; }
    
    // İlişkiler
    public Member? Member { get; set; }
}

