using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Rol zorunludur.")]
    public string Role { get; set; } = "Member"; // Admin veya Member
    
    [Required(ErrorMessage = "Oluşturulma tarihi zorunludur.")]
    public DateTime CreatedDate { get; set; }
    
    // Navigation properties
    public Member? Member { get; set; }
}

