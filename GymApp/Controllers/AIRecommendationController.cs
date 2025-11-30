using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers;

public class AIRecommendationController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

