using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Üyelerin yapay zeka ile yaptıkları sohbet mesajlarını temsil eder.
public class ChatMessage
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Mesajı gönderen üyenin kimliği (FK)
    [Required(ErrorMessage = "Üye seçimi zorunludur.")]
    public int MemberId { get; set; }
    
    // Kullanıcı tarafından gönderilen metin mesajı
    [Required(ErrorMessage = "Mesaj zorunludur.")]
    public string Message { get; set; } = string.Empty;
    
    // Yapay zekadan gelen cevap (opsiyonel)
    public string? Response { get; set; } // AI'dan gelen cevap
    
    // Mesajın oluşturulma tarihi
    [Required(ErrorMessage = "Oluşturulma tarihi zorunludur.")]
    public DateTime CreatedDate { get; set; }
    
    // İlişkiler
    public Member Member { get; set; } = null!;
}

