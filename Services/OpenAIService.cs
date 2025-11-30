using System.Text;
using System.Text.Json;

namespace GymApp.Services;

public class OpenAIService : IAIService, IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration["OpenAI:ApiKey"] ?? "";
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetChatResponseAsync(string userMessage, string? userContext = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return "OpenAI API anahtarı yapılandırılmamış. Lütfen sistem yöneticisine başvurun.";
        }

        var systemPrompt = @"Sen bir fitness ve sağlık danışmanısın. Kullanıcılara egzersiz, diyet ve sağlık konularında yardımcı oluyorsun. 
Türkçe cevap ver. Kısa, öz ve anlaşılır ol. Profesyonel ama samimi bir dil kullan.";

        if (!string.IsNullOrEmpty(userContext))
        {
            systemPrompt += $"\n\nKullanıcı bilgileri: {userContext}";
        }

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userMessage }
        };

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = messages,
            temperature = 0.7,
            max_tokens = 500
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrl, content);
            
            // Rate limit kontrolü
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return "Üzgünüm, şu anda çok fazla istek alındığı için yanıt veremiyorum. Lütfen birkaç dakika sonra tekrar deneyin.";
            }
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"API hatası: {response.StatusCode}. Lütfen daha sonra tekrar deneyin.";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseContent);

            return responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Yanıt alınamadı.";
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            return "Çok fazla istek gönderildi. Lütfen birkaç dakika bekleyip tekrar deneyin.";
        }
        catch (Exception ex)
        {
            // Daha kullanıcı dostu hata mesajları
            if (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests"))
            {
                return "Çok fazla istek gönderildi. Lütfen birkaç dakika bekleyip tekrar deneyin.";
            }
            
            return $"Bir hata oluştu. Lütfen daha sonra tekrar deneyin. (Hata: {ex.Message})";
        }
    }

    public async Task<string> GetExerciseRecommendationAsync(string bodyType, double? height = null, double? weight = null, string? photoDescription = null)
    {
        var context = $"Vücut tipi: {bodyType}";
        if (height.HasValue) context += $", Boy: {height} cm";
        if (weight.HasValue) context += $", Kilo: {weight} kg";
        if (!string.IsNullOrEmpty(photoDescription)) context += $", Fotoğraf açıklaması: {photoDescription}";

        var message = $"{context}\n\nBana uygun bir egzersiz planı öner. Haftalık program, egzersiz türleri, set ve tekrar sayıları dahil olmak üzere detaylı bir plan hazırla.";

        return await GetChatResponseAsync(message, context);
    }

    public async Task<string> GetDietRecommendationAsync(string bodyType, double? height = null, double? weight = null, string? photoDescription = null)
    {
        var context = $"Vücut tipi: {bodyType}";
        if (height.HasValue) context += $", Boy: {height} cm";
        if (weight.HasValue) context += $", Kilo: {weight} kg";
        if (!string.IsNullOrEmpty(photoDescription)) context += $", Fotoğraf açıklaması: {photoDescription}";

        var message = $"{context}\n\nBana uygun bir diyet planı öner. Günlük öğün planı, kalori hedefi, makro besin dağılımı ve örnek menüler dahil olmak üzere detaylı bir plan hazırla.";

        return await GetChatResponseAsync(message, context);
    }

    public async Task<string> GetVisualizationAsync(string exercisePlan, string bodyType)
    {
        var message = $"Aşağıdaki egzersiz planını takip edersem nasıl görüneceğimi açıkla:\n\n{exercisePlan}\n\nVücut tipim: {bodyType}\n\nBu planı takip ettikten sonra vücudumda ne gibi değişiklikler olacak? Hangi bölgelerde gelişme göreceğim?";

        return await GetChatResponseAsync(message);
    }
}

