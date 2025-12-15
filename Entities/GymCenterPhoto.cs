using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Spor salonlarına ait ek fotoğrafları temsil eder.
public class GymCenterPhoto
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // Fotoğrafın ait olduğu spor salonunun kimliği (FK)
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    // Fotoğraf URL bilgisi
    [Required(ErrorMessage = "Fotoğraf URL'i zorunludur.")]
    public string PhotoUrl { get; set; } = string.Empty;
    
    // İlişkiler
    public GymCenter GymCenter { get; set; } = null!;
}

