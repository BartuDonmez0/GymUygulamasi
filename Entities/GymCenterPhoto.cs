using System.ComponentModel.DataAnnotations;

namespace GymApp.Entities;

public class GymCenterPhoto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
    public int GymCenterId { get; set; }
    
    [Required(ErrorMessage = "Fotoğraf URL'i zorunludur.")]
    public string PhotoUrl { get; set; } = string.Empty;
    
    // Navigation property
    public GymCenter GymCenter { get; set; } = null!;
}

