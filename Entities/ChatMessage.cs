using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Üye seçimi zorunludur.")]
    public int MemberId { get; set; }
    
    [Required(ErrorMessage = "Mesaj zorunludur.")]
    public string Message { get; set; } = string.Empty;
    
    public string? Response { get; set; } // AI'dan gelen cevap
    
    [Required(ErrorMessage = "Oluşturulma tarihi zorunludur.")]
    public DateTime CreatedDate { get; set; }
    
    // Navigation property
    public Member Member { get; set; } = null!;
}

