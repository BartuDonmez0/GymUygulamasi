using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers;

public class TrainerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

