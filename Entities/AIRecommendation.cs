using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Üyelere ait yapay zeka önerilerini (egzersiz, diyet vb.) temsil eder.
public class AIRecommendation
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Önerinin ait olduğu üyenin kimliği (FK)
    [Required(ErrorMessage = "Üye seçimi zorunludur.")]
    public int MemberId { get; set; }
    
    // Öneri tipi (örnek: Exercise, Diet)
    [Required(ErrorMessage = "Öneri tipi zorunludur.")]
    public string RecommendationType { get; set; } = string.Empty; // Exercise, Diet, etc.
    
    // Metin içerik
    [Required(ErrorMessage = "İçerik zorunludur.")]
    public string Content { get; set; } = string.Empty;
    
    // Önerinin oluşturulma tarihi
    [Required(ErrorMessage = "Oluşturulma tarihi zorunludur.")]
    public DateTime CreatedDate { get; set; }
    
    // İlişkiler
    // Önerinin ait olduğu üye
    public Member Member { get; set; } = null!;
}

