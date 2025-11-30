using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace GymApp.Services;

/// <summary>
/// Gemini AI Service - Google Gemini API entegrasyonu için servis
/// AI Entegrasyonu: Gemini 2.0 Flash modeli ile yapay zeka destekli öneriler sağlar
/// Fotoğraf desteği: Base64 encoding ile fotoğraf yükleme ve analiz desteği
/// </summary>
public class GeminiAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    /// <summary>
    /// Constructor - Dependency injection ile HttpClient ve GeminiOptions'ı alır
    /// </summary>
    public GeminiAIService(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <summary>
    /// GetChatResponseAsync - AI chatbot'a mesaj gönderir ve yanıt alır
    /// AI Entegrasyonu: Gemini API ile genel sohbet desteği
    /// </summary>
    /// <param name="userMessage">Kullanıcı mesajı</param>
    /// <param name="userContext">Kullanıcı bağlam bilgileri (opsiyonel)</param>
    /// <returns>AI yanıtı</returns>
    public async Task<string> GetChatResponseAsync(string userMessage, string? userContext = null)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            return "Gemini API anahtarı yapılandırılmamış. Lütfen appsettings.json dosyasında 'Gemini:ApiKey' ayarını kontrol edin.";
        }

        var systemPrompt = @"Sen bir fitness ve sağlık danışmanısın. Kullanıcılara egzersiz, diyet ve sağlık konularında yardımcı oluyorsun. 
Türkçe cevap ver. Kısa, öz ve anlaşılır ol. Profesyonel ama samimi bir dil kullan.";

        if (!string.IsNullOrEmpty(userContext))
        {
            systemPrompt += $"\n\nKullanıcı bilgileri: {userContext}";
        }

        var fullPrompt = $"{systemPrompt}\n\nKullanıcı sorusu: {userMessage}";

        return await CallGeminiAPIAsync(fullPrompt, null);
    }

    /// <summary>
    /// GetExerciseRecommendationAsync - Özelleştirilmiş egzersiz planı önerisi alır
    /// AI Entegrasyonu: Vücut tipi, boy, kilo ve fotoğraf açıklamasına göre egzersiz planı oluşturur
    /// </summary>
    /// <param name="bodyType">Vücut tipi (Ektomorf, Mezomorf, Endomorf)</param>
    /// <param name="height">Boy (cm) (opsiyonel)</param>
    /// <param name="weight">Kilo (kg) (opsiyonel)</param>
    /// <param name="photoDescription">Fotoğraf açıklaması (opsiyonel)</param>
    /// <returns>Egzersiz planı önerisi</returns>
    public async Task<string> GetExerciseRecommendationAsync(string bodyType, double? height = null, double? weight = null, string? photoDescription = null)
    {
        var context = $"Vücut tipi: {bodyType}";
        if (height.HasValue) context += $", Boy: {height} cm";
        if (weight.HasValue) context += $", Kilo: {weight} kg";
        if (!string.IsNullOrEmpty(photoDescription)) context += $", Fotoğraf açıklaması: {photoDescription}";

        var promptText = $@"
Sen uzman bir spor ve beslenme koçusun.

Kullanıcının bilgileri:
- Vücut tipi: {bodyType}
- Boy: {(height.HasValue ? height + " cm" : "Belirtilmemiş")}
- Kilo: {(weight.HasValue ? weight + " kg" : "Belirtilmemiş")}
{(string.IsNullOrEmpty(photoDescription) ? "" : $"- Fotoğraf açıklaması: {photoDescription}")}

Kullanıcı için haftalık ayrıntılı bir egzersiz planı hazırla.
Gün gün (Pazartesi, Salı...) yaz. Isınma, ana egzersiz ve esneme kısımlarını da ekle.
Set ve tekrar sayılarını belirt.
Cevabı Türkçe ver.
";

        return await CallGeminiAPIAsync(promptText, null);
    }

    /// <summary>
    /// GetDietRecommendationAsync - Özelleştirilmiş diyet planı önerisi alır
    /// AI Entegrasyonu: Vücut tipi, boy, kilo ve fotoğraf açıklamasına göre diyet planı oluşturur
    /// </summary>
    /// <param name="bodyType">Vücut tipi (Ektomorf, Mezomorf, Endomorf)</param>
    /// <param name="height">Boy (cm) (opsiyonel)</param>
    /// <param name="weight">Kilo (kg) (opsiyonel)</param>
    /// <param name="photoDescription">Fotoğraf açıklaması (opsiyonel)</param>
    /// <returns>Diyet planı önerisi</returns>
    public async Task<string> GetDietRecommendationAsync(string bodyType, double? height = null, double? weight = null, string? photoDescription = null)
    {
        var context = $"Vücut tipi: {bodyType}";
        if (height.HasValue) context += $", Boy: {height} cm";
        if (weight.HasValue) context += $", Kilo: {weight} kg";
        if (!string.IsNullOrEmpty(photoDescription)) context += $", Fotoğraf açıklaması: {photoDescription}";

        var promptText = $@"
Sen uzman bir beslenme ve diyet koçusun.

Kullanıcının bilgileri:
- Vücut tipi: {bodyType}
- Boy: {(height.HasValue ? height + " cm" : "Belirtilmemiş")}
- Kilo: {(weight.HasValue ? weight + " kg" : "Belirtilmemiş")}
{(string.IsNullOrEmpty(photoDescription) ? "" : $"- Fotoğraf açıklaması: {photoDescription}")}

Kullanıcı için haftalık ayrıntılı bir diyet planı hazırla.
Günlük öğün planı (kahvaltı, öğle yemeği, akşam yemeği, ara öğünler), kalori hedefi, makro besin dağılımı ve örnek menüler dahil olmak üzere detaylı bir plan hazırla.
Cevabı Türkçe ver.
";

        return await CallGeminiAPIAsync(promptText, null);
    }

    /// <summary>
    /// GetVisualizationAsync - Egzersiz planına göre gelecekteki görünüm tahmini alır
    /// AI Entegrasyonu: Egzersiz planını analiz ederek vücut değişikliklerini tahmin eder
    /// </summary>
    /// <param name="exercisePlan">Egzersiz planı metni</param>
    /// <param name="bodyType">Vücut tipi</param>
    /// <returns>Görünüm tahmini</returns>
    public async Task<string> GetVisualizationAsync(string exercisePlan, string bodyType)
    {
        var message = $"Aşağıdaki egzersiz planını takip edersem nasıl görüneceğimi açıkla:\n\n{exercisePlan}\n\nVücut tipim: {bodyType}\n\nBu planı takip ettikten sonra vücudumda ne gibi değişiklikler olacak? Hangi bölgelerde gelişme göreceğim?";

        return await GetChatResponseAsync(message);
    }

    /// <summary>
    /// GetWorkoutPlanWithPhotoAsync - Fotoğraf ile birlikte egzersiz planı önerisi alır
    /// AI Entegrasyonu: Fotoğraf yükleme desteği ile görsel analiz yaparak özelleştirilmiş plan oluşturur
    /// Fotoğraf desteği: Base64 encoding ile fotoğraf gönderimi
    /// </summary>
    /// <param name="bodyType">Vücut tipi</param>
    /// <param name="height">Boy (cm) (opsiyonel)</param>
    /// <param name="weight">Kilo (kg) (opsiyonel)</param>
    /// <param name="goal">Hedef (örn: "Fitness", "Kilo verme")</param>
    /// <param name="photoFile">Yüklenen fotoğraf (opsiyonel)</param>
    /// <returns>Egzersiz planı önerisi</returns>
    public async Task<string> GetWorkoutPlanWithPhotoAsync(
        string bodyType,
        int? height,
        int? weight,
        string goal,
        IFormFile? photoFile)
    {
        var promptText = $@"
Sen uzman bir spor ve beslenme koçusun.

Kullanıcının bilgileri:
- Vücut tipi: {bodyType}
- Boy: {(height.HasValue ? height + " cm" : "Belirtilmemiş")}
- Kilo: {(weight.HasValue ? weight + " kg" : "Belirtilmemiş")}
- Hedef: {goal}

Kullanıcı için haftalık ayrıntılı bir egzersiz planı ve kısa beslenme önerisi hazırla.
Gün gün (Pazartesi, Salı...) yaz. Isınma, ana egzersiz ve esneme kısımlarını da ekle.
Cevabı Türkçe ver.
";

        return await CallGeminiAPIAsync(promptText, photoFile);
    }

    /// <summary>
    /// CallGeminiAPIAsync - Gemini API'ye istek gönderir ve yanıt alır
    /// AI Entegrasyonu: HTTP POST isteği ile Gemini API'ye bağlanır
    /// Fotoğraf desteği: Base64 encoding ile fotoğraf gönderimi
    /// Hata yönetimi: API hatalarını yakalar ve kullanıcı dostu mesajlar döndürür
    /// </summary>
    /// <param name="promptText">AI'ya gönderilecek metin</param>
    /// <param name="photoFile">Yüklenen fotoğraf (opsiyonel)</param>
    /// <returns>AI yanıtı</returns>
    private async Task<string> CallGeminiAPIAsync(string promptText, IFormFile? photoFile)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            return "Gemini API anahtarı yapılandırılmamış. Lütfen appsettings.json dosyasında 'Gemini:ApiKey' ayarını kontrol edin.";
        }

        var parts = new List<object>
        {
            new { text = promptText }
        };

        // Fotoğraf yüklendiyse Base64'e çevirip inline_data olarak ekle
        if (photoFile != null && photoFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await photoFile.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var base64 = Convert.ToBase64String(bytes);

            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = photoFile.ContentType ?? "image/jpeg",
                    data = base64
                }
            });
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = parts.ToArray()
                }
            },
            generationConfig = new
            {
                temperature = 0.8,
                maxOutputTokens = 2048
            }
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorJson = JsonDocument.Parse(errorContent);
                    if (errorJson.RootElement.TryGetProperty("error", out var errorElement))
                    {
                        var errorMessage = errorElement.TryGetProperty("message", out var msgElement)
                            ? msgElement.GetString()
                            : errorElement.GetRawText();
                        return $"API hatası: {errorMessage}";
                    }
                }
                catch
                {
                    // JSON parse edilemediyse ham hata mesajını döndür
                }
                return $"Üzgünüm, şu an yapay zekâ servisinden cevap alınamadı. (HTTP {response.StatusCode})";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);

            // Gemini cevabındaki ilk text parçasını yakala
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                    contentElement.TryGetProperty("parts", out var partsElement) &&
                    partsElement.GetArrayLength() > 0 &&
                    partsElement[0].TryGetProperty("text", out var textElement))
                {
                    return textElement.GetString() ?? "Herhangi bir yanıt alınamadı.";
                }
            }

            return "Yanıt formatı beklenenden farklı.";
        }
        catch (Exception ex)
        {
            return $"Bir hata oluştu: {ex.Message}";
        }
    }
}
