using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers;

public class AppointmentController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

