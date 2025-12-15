using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymApp.Entities;
using GymApp.Data;
using GymApp.Services;

namespace GymApp.Controllers;

// Ana sayfa, gizlilik ve veritabanı test ekranlarını yöneten controller.
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly GymAppDbContext _context;

    // Constructor - logger ve DbContext bağımlılıklarını alır.
    public HomeController(ILogger<HomeController> logger, GymAppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET: /Home/Index - Ana sayfayı gösterir.
    public IActionResult Index()
    {
        return View();
    }

    // GET: /Home/Privacy - Gizlilik metni sayfasını gösterir.
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Uygulama hata sayfasını gösterir.
    public IActionResult Error()
    {
        return View(new Entities.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Veritabanı bağlantısını ve temel tabloları test etmek için.
    public async Task<IActionResult> TestDatabase()
    {
        try
        {
            // Veritabanı bağlantısını test et
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                // Tabloları kontrol et
                var membersCount = await _context.Members.CountAsync();
                var trainersCount = await _context.Trainers.CountAsync();
                var gymCentersCount = await _context.GymCenters.CountAsync();
                
                ViewBag.Message = "✅ Veritabanı bağlantısı başarılı!";
                ViewBag.MembersCount = membersCount;
                ViewBag.TrainersCount = trainersCount;
                ViewBag.GymCentersCount = gymCentersCount;
                ViewBag.Status = "success";
            }
            else
            {
                ViewBag.Message = "❌ Veritabanına bağlanılamadı!";
                ViewBag.Status = "error";
            }
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"❌ Hata: {ex.Message}";
            ViewBag.Status = "error";
            ViewBag.ErrorDetails = ex.ToString();
        }
        
        return View();
    }
}
