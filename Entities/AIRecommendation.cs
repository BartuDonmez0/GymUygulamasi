using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class AIRecommendation
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Üye seçimi zorunludur.")]
    public int MemberId { get; set; }
    
    [Required(ErrorMessage = "Öneri tipi zorunludur.")]
    public string RecommendationType { get; set; } = string.Empty; // Exercise, Diet, etc.
    
    [Required(ErrorMessage = "İçerik zorunludur.")]
    public string Content { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Oluşturulma tarihi zorunludur.")]
    public DateTime CreatedDate { get; set; }
    
    // Navigation property
    public Member Member { get; set; } = null!;
}

