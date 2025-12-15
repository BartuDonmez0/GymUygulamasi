namespace GymApp.Entities;

// JSON formatında saklanan çalışma saati kayıtları için yardımcı model.
public class WorkingHoursJson
{
    // Haftanın günü (0=Pazar, 1=Pazartesi, ..., 6=Cumartesi)
    public int Day { get; set; }

    // Saat aralığı metni (örnek: "09:00-18:00")
    public string TimeRange { get; set; } = string.Empty;
}

