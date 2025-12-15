using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymApp.Services;
using GymApp.Repositories;

namespace GymApp.Controllers;

// Yapay zeka ile egzersiz/diyet önerilerini yöneten controller.
[Authorize(Roles = "User")] // Rol bazlı yetkilendirme: Sadece User rolü erişebilir (Admin erişemez)
public class AIRecommendationController : Controller
{
    private readonly IChatMessageService _chatMessageService;
    private readonly IAIService _aiService;
    private readonly IMemberRepository _memberRepository;

    // Constructor - chat, AI ve üye repository bağımlılıklarını alır.
    public AIRecommendationController(
        IChatMessageService chatMessageService,
        IAIService aiService,
        IMemberRepository memberRepository)
    {
        _chatMessageService = chatMessageService;
        _aiService = aiService;
        _memberRepository = memberRepository;
    }

    // GET: /AIRecommendation - Kullanıcının AI öneri ekranını ve geçmiş sohbetlerini gösterir.
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
    
    // GET: /AIRecommendation/TestAI - Gemini API yapılandırmasını test eder.
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

    // GET: /AIRecommendation/ListModels - Mevcut Gemini modellerini listeler (test için).
    [HttpGet]
    public async Task<IActionResult> ListModels()
    {
        var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = configuration["Gemini:ApiKey"];
        
        if (string.IsNullOrEmpty(apiKey))
        {
            return Json(new { success = false, message = "API anahtarı bulunamadı." });
        }

        try
        {
            using var httpClient = new HttpClient();
            
            // v1beta API'sinden modelleri listele
            var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
            var response = await httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Json(new { success = true, data = content, apiVersion = "v1beta" });
            }
            else
            {
                // v1 API'sini dene
                url = $"https://generativelanguage.googleapis.com/v1/models?key={apiKey}";
                response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, data = content, apiVersion = "v1" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = errorContent });
                }
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: /AIRecommendation/SendMessage - Sohbet mesajı gönderip AI cevabını döndürür.
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

    // POST: /AIRecommendation/GetExerciseRecommendation - AI'dan egzersiz planı ister.
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

    // POST: /AIRecommendation/GetDietRecommendation - AI'dan diyet planı ister.
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

    // POST: /AIRecommendation/ClearChat - Kullanıcının tüm sohbet geçmişini siler.
    [HttpPost]
    public async Task<IActionResult> ClearChat()
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
            await _chatMessageService.DeleteChatMessagesByMemberIdAsync(member.Id);
            return Json(new { success = true, message = "Sohbet geçmişi temizlendi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }
}
