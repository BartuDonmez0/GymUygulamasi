using System.ComponentModel.DataAnnotations;

namespace GymApp.Areas.Admin.ViewModels;

public class AdminMessagesViewModel
{
    public List<AdminMessageItemViewModel> Inbox { get; set; } = [];
    public AdminMessageReply Reply { get; set; } = new();
}

public class AdminMessageItemViewModel
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public bool IsRead { get; set; }
}

public class AdminMessageReply
{
    [Required(ErrorMessage = "Alıcı e-postası zorunludur..")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string ToEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konu zorunludur.")]
    [StringLength(120, ErrorMessage = "Konu en fazla 120 karakter olabilir.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mesaj içeriği zorunludur.")]
    [StringLength(1000, ErrorMessage = "Mesaj en fazla 1000 karakter olabilir.")]
    public string Body { get; set; } = string.Empty;
}

