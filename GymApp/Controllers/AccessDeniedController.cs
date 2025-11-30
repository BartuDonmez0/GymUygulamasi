using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers;

/// <summary>
/// AccessDenied Controller - Yetkisiz erişim durumlarını yönetir
/// Authorization: Yetkisiz erişim denemelerinde gösterilir
/// </summary>
public class AccessDeniedController : Controller
{
    /// <summary>
    /// Index - Erişim reddedildi sayfasını gösterir
    /// Authorization: Yetkisiz erişim durumunda gösterilir
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }
}

