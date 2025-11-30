namespace GymApp.Entities;

// JSON formatında çalışma saatleri için model
public class WorkingHoursJson
{
    public int Day { get; set; } // 0=Pazar, 1=Pazartesi, ..., 6=Cumartesi
    public string TimeRange { get; set; } = string.Empty; // "09:00-18:00" formatında
}

