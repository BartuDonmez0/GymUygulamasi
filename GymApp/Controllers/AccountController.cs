using Microsoft.AspNetCore.Mvc;
using GymApp.Entities;
using GymApp.Services;

namespace GymApp.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private const string AdminEmail = "G231210561@sakarya.edu.tr";
    private const string AdminPassword = "sau";

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "E-posta ve şifre alanları zorunludur.";
            return View();
        }

        // Admin kontrolü
        if (_accountService.IsAdmin(email, password))
        {
            // Admin girişi - Session veya cookie ile yönetilebilir
            HttpContext.Session.SetString("IsAdmin", "true");
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserName", "Admin");
            
            // Hoş geldin pop-up için cookie set et
            Response.Cookies.Append("ShowWelcomeMessage", "true", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(1),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            });
            Response.Cookies.Append("WelcomeUserName", "Admin", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(1),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            });
            
            return RedirectToAction("Index", "Admin", new { area = "Admin" });
        }

        // Normal kullanıcı kontrolü (Member)
        var member = await _accountService.LoginAsync(email, password);

        if (member != null)
        {
            HttpContext.Session.SetString("IsAdmin", "false");
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserName", $"{member.FirstName} {member.LastName}");
            HttpContext.Session.SetInt32("UserId", member.Id);
            
            // Hoş geldin pop-up için cookie set et
            Response.Cookies.Append("ShowWelcomeMessage", "true", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(1),
                HttpOnly = false, // JavaScript'ten erişilebilir olmalı
                SameSite = SameSiteMode.Lax
            });
            
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "E-posta veya şifre hatalı.";
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Member model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Email kontrolü - aynı email ile kayıt olamaz
        if (await _accountService.IsEmailExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor. Lütfen farklı bir e-posta adresi giriniz.");
            return View(model);
        }

        // Yeni üye oluştur
        var registeredMember = await _accountService.RegisterAsync(model);
        
        if (registeredMember == null)
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi ile kayıt olamazsınız.");
            return View(model);
        }
        
        model = registeredMember;

        // Kayıt sonrası otomatik giriş yap
        HttpContext.Session.SetString("IsAdmin", "false");
        HttpContext.Session.SetString("UserEmail", model.Email);
        HttpContext.Session.SetString("UserName", $"{model.FirstName} {model.LastName}");
        HttpContext.Session.SetInt32("UserId", model.Id);
        
        // Hoş geldin pop-up için cookie set et
        Response.Cookies.Append("ShowWelcomeMessage", "true", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            HttpOnly = false, // JavaScript'ten erişilebilir olmalı
            SameSite = SameSiteMode.Lax
        });
        
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login");
        }

        // Admin kontrolü
        if (isAdmin == "true")
        {
            // Admin için basit bir model oluştur
            var adminModel = new Member
            {
                FirstName = "Admin",
                LastName = "",
                Email = userEmail ?? "",
                Phone = "",
                RegistrationDate = DateTime.UtcNow
            };
            return View(adminModel);
        }

        // Normal kullanıcı kontrolü
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var member = await _accountService.GetMemberByIdAsync(userId.Value);

        if (member == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        return View(member);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Append("LogoutMessage", "Başarıyla çıkış yaptınız.", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });
        return RedirectToAction("Index", "Home");
    }
}

