using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using GymApp.Entities;
using GymApp.Services;

namespace GymApp.Controllers;

// Kullanıcı giriş, kayıt, profil görüntüleme ve çıkış işlemlerini yöneten controller.
public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    // Admin kullanıcı bilgileri - rol bazlı yetkilendirme için sabit değerler
    private const string AdminEmail = "G231210561@sakarya.edu.tr";
    private const string AdminPassword = "sau";

    // Constructor - IAccountService bağımlılığını alır.
    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // GET: /Account/Login - Giriş formunu gösterir.
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login - Kullanıcıyı e‑posta/şifre ile sisteme giriş yaptırır.
    [HttpPost]
    [ValidateAntiForgeryToken] // CSRF koruması - Güvenlik için anti-forgery token kontrolü
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
            // Admin girişi - Session ve Claims ile yönetilir
            HttpContext.Session.SetString("IsAdmin", "true");
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserName", "Admin");
            
            // Claims-based authentication - Admin rolü için claim ekle
            // ClaimTypes.Role kullanılıyor - [Authorize(Roles="Admin")] için gerekli
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, "Admin"), // Rol claim'i - [Authorize(Roles="Admin")] için (ClaimTypes.Role kullanılmalı)
                new Claim("UserId", "0") // Admin için özel ID
            };
            
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);
            
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
            
            // Claims-based authentication - User rolü için claim ekle
            // ClaimTypes.Role kullanılıyor - [Authorize(Roles="User")] için gerekli
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{member.FirstName} {member.LastName}"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, "User"), // Rol claim'i - [Authorize(Roles="User")] için (ClaimTypes.Role kullanılmalı)
                new Claim("UserId", member.Id.ToString())
            };
            
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);
            
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

    // GET: /Account/Register - Kayıt formunu gösterir.
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register - Yeni kullanıcı kaydı oluşturur.
    [HttpPost]
    [ValidateAntiForgeryToken] // CSRF koruması
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
        
        // Claims-based authentication - User rolü için claim ekle
        // ClaimTypes.Role kullanılıyor - [Authorize(Roles="User")] için gerekli
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"{model.FirstName} {model.LastName}"),
            new Claim(ClaimTypes.Email, model.Email),
            new Claim(ClaimTypes.Role, "User"), // Rol claim'i - [Authorize(Roles="User")] için (ClaimTypes.Role kullanılmalı)
            new Claim("UserId", model.Id.ToString())
        };
        
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };
        
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);
        
        // Hoş geldin pop-up için cookie set et
        Response.Cookies.Append("ShowWelcomeMessage", "true", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            HttpOnly = false, // JavaScript'ten erişilebilir olmalı
            SameSite = SameSiteMode.Lax
        });
        
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Profile - Oturum açmış kullanıcının profilini gösterir.
    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        
        // Authorization kontrolü: Giriş yapmamış kullanıcılar login sayfasına yönlendirilir
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login");
        }

        // Rol bazlı yetkilendirme: Admin kullanıcıları için özel model
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

    // GET: /Account/AccessDenied - Yetkisiz erişim sayfasını gösterir.
    public IActionResult AccessDenied()
    {
        return View();
    }

    // GET: /Account/Logout - Kullanıcıyı sistemden çıkarır.
    public async Task<IActionResult> Logout()
    {
        // Authentication cookie'yi temizle
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        HttpContext.Session.Clear();
        Response.Cookies.Append("ShowLogoutMessage", "true", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            HttpOnly = false, // JavaScript'ten erişilebilir olmalı
            SameSite = SameSiteMode.Lax
        });
        return RedirectToAction("Index", "Home");
    }

    // POST: /Account/DeleteProfile - Üyenin kendi profilini silmesini sağlar (Admin hariç).
    [HttpPost]
    [ValidateAntiForgeryToken] // CSRF koruması
    [Authorize(Roles = "User")] // Rol bazlı yetkilendirme: Sadece User rolü erişebilir (Admin erişemez)
    public async Task<IActionResult> DeleteProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var isAdmin = HttpContext.Session.GetString("IsAdmin");

        if (string.IsNullOrEmpty(userEmail) || userId == null)
        {
            return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
        }

        // Admin silinemez
        if (isAdmin == "true")
        {
            return Json(new { success = false, message = "Admin hesabı silinemez." });
        }

        try
        {
            var deleted = await _accountService.DeleteMemberAsync(userId.Value);
            
            if (deleted)
            {
                // Session'ı temizle
                HttpContext.Session.Clear();
                
                return Json(new { success = true, message = "Profiliniz başarıyla silindi." });
            }
            else
            {
                return Json(new { success = false, message = "Profil silinirken bir hata oluştu." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }
}

