using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymApp.Services;
using GymApp.Repositories;

namespace GymApp.Controllers;

/// <summary>
/// AIRecommendation Controller - Yapay zeka destekli fitness danışmanı işlemlerini yönetir
/// AI Entegrasyonu: Gemini API kullanarak egzersiz ve diyet önerileri sağlar
/// Authorization: Giriş yapmış kullanıcılar erişebilir
/// </summary>
[Authorize(Roles = "User")] // Rol bazlı yetkilendirme: Sadece User rolü erişebilir (Admin erişemez)
public class AIRecommendationController : Controller
{
    private readonly IChatMessageService _chatMessageService;
    private readonly IAIService _aiService;
    private readonly IMemberRepository _memberRepository;

    /// <summary>
    /// Constructor - Dependency injection ile servisleri alır
    /// </summary>
    public AIRecommendationController(
        IChatMessageService chatMessageService,
        IAIService aiService,
        IMemberRepository memberRepository)
    {
        _chatMessageService = chatMessageService;
        _aiService = aiService;
        _memberRepository = memberRepository;
    }

    /// <summary>
    /// Index - AI önerileri sayfasını gösterir
    /// Authorization: Giriş yapmış kullanıcılar erişebilir
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // Giriş kontrolü
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login", "Account");
        }

        var member = await _memberRepository.GetByEmailAsync(userEmail);
        if (member == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Kullanıcının önceki mesajlarını getir
        var chatMessages = await _chatMessageService.GetChatMessagesByMemberIdAsync(member.Id);
        ViewBag.ChatMessages = chatMessages;
        ViewBag.MemberId = member.Id;

        return View();
    }
    
    /// <summary>
    /// TestAI - AI API bağlantısını test eder
    /// Test endpoint: API key yapılandırmasını kontrol eder
    /// </summary>
    [HttpGet]
    public IActionResult TestAI()
    {
        // Admin veya test için AI bağlantısını test et
        var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = configuration["Gemini:ApiKey"];
        
        return Json(new { 
            provider = "Gemini", 
            hasApiKey = !string.IsNullOrEmpty(apiKey),
            apiKeyLength = apiKey?.Length ?? 0
        });
    }

    /// <summary>
    /// SendMessage - AI chatbot'a mesaj gönderir ve yanıt alır
    /// AI Entegrasyonu: Gemini API ile sohbet yapar
    /// Fotoğraf desteği: IFormFile ile fotoğraf yükleme desteği
    /// </summary>
    /// <param name="message">Kullanıcı mesajı</param>
    /// <param name="bodyType">Vücut tipi (opsiyonel)</param>
    /// <param name="height">Boy (cm) (opsiyonel)</param>
    /// <param name="weight">Kilo (kg) (opsiyonel)</param>
    /// <param name="photoDescription">Fotoğraf açıklaması (opsiyonel)</param>
    /// <param name="photoFile">Yüklenen fotoğraf (opsiyonel)</param>
    [HttpPost]
    public async Task<IActionResult> SendMessage(string message, string? bodyType = null, double? height = null, double? weight = null, string? photoDescription = null, IFormFile? photoFile = null)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
        }

        var member = await _memberRepository.GetByEmailAsync(userEmail);
        if (member == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return Json(new { success = false, message = "Mesaj boş olamaz." });
        }

        try
        {
            // Kullanıcı bağlamı oluştur
            var userContext = $"Kullanıcı: {member.FirstName} {member.LastName}";
            if (!string.IsNullOrEmpty(bodyType))
            {
                userContext += $", Vücut tipi: {bodyType}";
            }
            if (height.HasValue)
            {
                userContext += $", Boy: {height} cm";
            }
            if (weight.HasValue)
            {
                userContext += $", Kilo: {weight} kg";
            }
            if (!string.IsNullOrEmpty(photoDescription))
            {
                userContext += $", Fotoğraf açıklaması: {photoDescription}";
            }

            // Mesajı kaydet
            var chatMessage = await _chatMessageService.CreateChatMessageAsync(member.Id, message);

            // AI'dan yanıt al
            string aiResponse;
            try
            {
                // GeminiAIService'e fotoğraf desteği eklemek için cast ediyoruz
                var geminiService = _aiService as GeminiAIService;
                
                if (message.ToLower().Contains("egzersiz") || message.ToLower().Contains("antrenman") || message.ToLower().Contains("spor"))
                {
                    if (photoFile != null && geminiService != null)
                    {
                        aiResponse = await geminiService.GetWorkoutPlanWithPhotoAsync(
                            bodyType ?? "Belirtilmemiş",
                            height.HasValue ? (int)height.Value : null,
                            weight.HasValue ? (int)weight.Value : null,
                            "Fitness",
                            photoFile);
                    }
                    else
                    {
                        aiResponse = await _aiService.GetExerciseRecommendationAsync(
                            bodyType ?? "Belirtilmemiş",
                            height,
                            weight,
                            photoDescription);
                    }
                }
                else if (message.ToLower().Contains("diyet") || message.ToLower().Contains("beslenme") || message.ToLower().Contains("yemek"))
                {
                    aiResponse = await _aiService.GetDietRecommendationAsync(
                        bodyType ?? "Belirtilmemiş",
                        height,
                        weight,
                        photoDescription);
                }
                else if (message.ToLower().Contains("görünüm") || message.ToLower().Contains("nasıl görüneceğim") || message.ToLower().Contains("sonuç"))
                {
                    aiResponse = await _aiService.GetVisualizationAsync(message, bodyType ?? "Belirtilmemiş");
                }
                else
                {
                    aiResponse = await _aiService.GetChatResponseAsync(message, userContext);
                }

                // Yanıtı güncelle
                await _chatMessageService.UpdateChatMessageResponseAsync(chatMessage.Id, aiResponse);

                return Json(new { success = true, response = aiResponse, messageId = chatMessage.Id });
            }
            catch (Exception aiEx)
            {
                // AI servis hatası durumunda bile mesajı kaydet
                var errorResponse = aiEx.Message.Contains("429") || aiEx.Message.Contains("Too Many Requests")
                    ? "Çok fazla istek gönderildi. Lütfen birkaç dakika bekleyip tekrar deneyin."
                    : $"AI servisi hatası: {aiEx.Message}";
                
                await _chatMessageService.UpdateChatMessageResponseAsync(chatMessage.Id, errorResponse);
                return Json(new { success = true, response = errorResponse, messageId = chatMessage.Id });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    /// <summary>
    /// GetExerciseRecommendation - Egzersiz planı önerisi alır
    /// AI Entegrasyonu: Gemini API ile özelleştirilmiş egzersiz planı oluşturur
    /// Fotoğraf desteği: Fotoğraf yüklendiğinde AI analiz eder
    /// </summary>
    /// <param name="bodyType">Vücut tipi</param>
    /// <param name="height">Boy (cm) (opsiyonel)</param>
    /// <param name="weight">Kilo (kg) (opsiyonel)</param>
    /// <param name="photoDescription">Fotoğraf açıklaması (opsiyonel)</param>
    /// <param name="photoFile">Yüklenen fotoğraf (opsiyonel)</param>
    [HttpPost]
    public async Task<IActionResult> GetExerciseRecommendation(string bodyType, double? height = null, double? weight = null, string? photoDescription = null, IFormFile? photoFile = null)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
        }

        var member = await _memberRepository.GetByEmailAsync(userEmail);
        if (member == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        try
        {
            var message = "Bana uygun bir egzersiz planı öner";
            var chatMessage = await _chatMessageService.CreateChatMessageAsync(member.Id, message);

            string recommendation;
            var geminiService = _aiService as GeminiAIService;
            
            if (photoFile != null && geminiService != null)
            {
                recommendation = await geminiService.GetWorkoutPlanWithPhotoAsync(
                    bodyType ?? "Belirtilmemiş",
                    height.HasValue ? (int)height.Value : null,
                    weight.HasValue ? (int)weight.Value : null,
                    "Fitness",
                    photoFile);
            }
            else
            {
                recommendation = await _aiService.GetExerciseRecommendationAsync(
                    bodyType,
                    height,
                    weight,
                    photoDescription);
            }

            await _chatMessageService.UpdateChatMessageResponseAsync(chatMessage.Id, recommendation);

            return Json(new { success = true, response = recommendation });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    /// <summary>
    /// GetDietRecommendation - Diyet planı önerisi alır
    /// AI Entegrasyonu: Gemini API ile özelleştirilmiş diyet planı oluşturur
    /// </summary>
    /// <param name="bodyType">Vücut tipi</param>
    /// <param name="height">Boy (cm) (opsiyonel)</param>
    /// <param name="weight">Kilo (kg) (opsiyonel)</param>
    /// <param name="photoDescription">Fotoğraf açıklaması (opsiyonel)</param>
    [HttpPost]
    public async Task<IActionResult> GetDietRecommendation(string bodyType, double? height = null, double? weight = null, string? photoDescription = null)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
        }

        var member = await _memberRepository.GetByEmailAsync(userEmail);
        if (member == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        try
        {
            var message = "Bana uygun bir diyet planı öner";
            var chatMessage = await _chatMessageService.CreateChatMessageAsync(member.Id, message);

            var recommendation = await _aiService.GetDietRecommendationAsync(
                bodyType,
                height,
                weight,
                photoDescription);

            await _chatMessageService.UpdateChatMessageResponseAsync(chatMessage.Id, recommendation);

            return Json(new { success = true, response = recommendation });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }
}
