namespace GymApp.Entities;

// Hata sayfasında gösterilen istek kimliği bilgisini tutan view modeli.
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

