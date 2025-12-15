using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers;

// Yetkisiz erişim durumlarında gösterilen sayfayı yöneten controller.
public class AccessDeniedController : Controller
{
    // GET: /AccessDenied/Index - Erişim reddedildi sayfasını gösterir.
    public IActionResult Index()
    {
        return View();
    }
}

