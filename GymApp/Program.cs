using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using GymApp.Data;
using GymApp.Repositories;
using GymApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVİS YAPILANDIRMASI (Dependency Injection)
// ============================================

// Add services to the container.
// MVC ve API controller desteği eklenir - Front-End ve REST API desteği
builder.Services.AddControllersWithViews();
builder.Services.AddControllers(); // REST API için controller desteği

// Session configuration - Kullanıcı oturum yönetimi için
// Rol bazlı yetkilendirme: Session'da kullanıcı bilgileri ve rol bilgisi saklanır
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authorization yapılandırması - Rol bazlı yetkilendirme için
// Claims-based authorization: Admin ve User rolleri için policy tanımlaması
// Not: [Authorize(Roles="Admin")] attribute'u ClaimTypes.Role claim'ini kullanır
builder.Services.AddAuthorization(options =>
{
    // Admin rolü için policy (ClaimTypes.Role kullanılıyor)
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    // User rolü için policy (ClaimTypes.Role kullanılıyor)
    options.AddPolicy("UserOnly", policy => 
        policy.RequireRole("User"));
    
    // Admin veya User rolleri için policy (ClaimTypes.Role kullanılıyor)
    options.AddPolicy("AdminOrUser", policy => 
        policy.RequireRole("Admin", "User"));
});

// Authentication yapılandırması - Claims-based authentication
// Cookie authentication: Rol bilgilerini cookie'de saklar
// Scheme adı "Cookies" olarak ayarlandı (CookieAuthenticationDefaults.AuthenticationScheme ile uyumlu)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// PostgreSQL DbContext configuration - Veritabanı bağlantısı
// Entity Framework Core Code First yaklaşımı ile veritabanı yönetimi
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<GymAppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Gemini konfigürasyonu - AI Entegrasyonu için yapılandırma
// AI Entegrasyonu: Gemini API key ve model ayarları
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection("Gemini"));

// HttpClient + Gemini servisi - AI Entegrasyonu için HTTP client
// AI Entegrasyonu: Gemini API ile iletişim için HttpClient yapılandırması
builder.Services.AddHttpClient<GeminiAIService>();
builder.Services.AddScoped<IAIService, GeminiAIService>();

// ============================================
// REPOSITORY PATTERN - Dependency Injection
// ============================================
// Veri erişim katmanı - CRUD işlemleri için repository'ler
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ITrainerRepository, TrainerRepository>();
builder.Services.AddScoped<IGymCenterRepository, GymCenterRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAIRecommendationRepository, AIRecommendationRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// ============================================
// SERVICE LAYER - Dependency Injection
// ============================================
// İş mantığı katmanı - Business logic için servisler
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();
builder.Services.AddScoped<IGymCenterService, GymCenterService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();

var app = builder.Build();

// ============================================
// HTTP REQUEST PIPELINE YAPILANDIRMASI
// ============================================

// Configure the HTTP request pipeline.
// Hata yönetimi: Production ortamında hata sayfasına yönlendirme
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection(); // HTTPS yönlendirmesi
app.UseRouting(); // Routing middleware

app.UseSession(); // Session middleware - Rol bazlı yetkilendirme için gerekli
app.UseAuthentication(); // Authentication middleware - Kimlik doğrulama kontrolü
app.UseAuthorization(); // Authorization middleware - Yetkilendirme kontrolü (Rol bazlı)

app.MapStaticAssets(); // Static dosyalar (CSS, JS, images)

// Admin Area route mapping - Admin paneli için route tanımlaması
// Admin paneli: Areas/Admin yapısı ile admin işlemleri
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}")
    .WithStaticAssets();

// Default MVC route mapping - Front-End sayfaları için route tanımlaması
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// REST API route mapping - API endpoint'leri için route tanımlaması
// REST API: /api/api/* endpoint'leri için route tanımlaması
// LINQ sorguları ile filtreleme: API controller'da LINQ sorguları kullanılır
app.MapControllers();

app.Run();
