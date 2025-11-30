using Microsoft.AspNetCore.Mvc;
using GymApp.Entities;
using GymApp.Services;

namespace GymApp.Controllers;

public class GymCenterController : Controller
{
    private readonly IGymCenterService _gymCenterService;

    public GymCenterController(IGymCenterService gymCenterService)
    {
        _gymCenterService = gymCenterService;
    }

    public async Task<IActionResult> Index()
    {
        var gymCenters = await _gymCenterService.GetAllGymCentersAsync();
        return View(gymCenters);
    }

    public async Task<IActionResult> Details(int id)
    {
        var gymCenter = await _gymCenterService.GetGymCenterWithDetailsAsync(id);
        if (gymCenter == null)
        {
            return NotFound();
        }
        return View(gymCenter);
    }

    public async Task<IActionResult> Activities(int id)
    {
        var gymCenter = await _gymCenterService.GetGymCenterByIdAsync(id);
        if (gymCenter == null)
        {
            return NotFound();
        }
        return RedirectToAction("Index", "Activity", new { gymCenterId = id });
    }
}

