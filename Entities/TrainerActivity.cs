namespace GymApp.Entities;

// Many-to-many relationship table between Trainer and Activity
public class TrainerActivity
{
    public int Id { get; set; }
    
    public int TrainerId { get; set; }
    public int ActivityId { get; set; }
    
    // Navigation properties
    public Trainer Trainer { get; set; } = null!;
    public Activity Activity { get; set; } = null!;
}

