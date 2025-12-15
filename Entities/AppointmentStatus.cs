namespace GymApp.Entities;

// Randevuların durumlarını (beklemede, onaylı vb.) temsil eden enum.
public enum AppointmentStatus
{
    Pending = 1,    // Beklemede
    Approved = 2,   // Onaylandı
    Rejected = 3,   // Reddedildi
    Completed = 4   // Tamamlandı
}

