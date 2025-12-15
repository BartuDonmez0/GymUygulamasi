using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Entities;

// Antrenörler ile aktiviteler arasındaki çoktan çoğa ilişkiyi temsil eder.
public class TrainerActivity
{
    // Birincil anahtar (PK)
    [Key]
    public int Id { get; set; }
    
    // İlişkinin ait olduğu antrenörün kimliği (FK)
    public int TrainerId { get; set; }
    // İlişkinin ait olduğu aktivitenin kimliği (FK)
    public int ActivityId { get; set; }
    
    // İlişkiler
    public Trainer Trainer { get; set; } = null!;
    public Activity Activity { get; set; } = null!;
}

