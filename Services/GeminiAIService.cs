using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace GymApp.Services;

// Google Gemini API ile yapay zeka entegrasyonunu yöneten servis.
public class GeminiAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    // Constructor - HttpClient ve GeminiOptions bağımlılıklarını alır.
    public GeminiAIService(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    // Genel sohbet için Gemini modelinden yanıt döndürür.
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

    // Kullanıcının vücut bilgilerine göre egzersiz planı önerisi üretir.
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

    // Kullanıcının vücut bilgilerine göre diyet planı önerisi üretir.
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

    // Egzersiz planına göre vücuttaki olası değişimleri metinsel olarak açıklar.
    public async Task<string> GetVisualizationAsync(string exercisePlan, string bodyType)
    {
        var message = $"Aşağıdaki egzersiz planını takip edersem nasıl görüneceğimi açıkla:\n\n{exercisePlan}\n\nVücut tipim: {bodyType}\n\nBu planı takip ettikten sonra vücudumda ne gibi değişiklikler olacak? Hangi bölgelerde gelişme göreceğim?";

        return await GetChatResponseAsync(message);
    }

    // Fotoğraf ve ölçülerle birlikte egzersiz planı üretir.
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

    // Gemini API'ye HTTP isteği gönderip gelen cevabı çözen yardımcı metot.
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

        // Retry mekanizması - maksimum 5 deneme (rate limit için daha fazla deneme)
        int maxRetries = 5;
        int baseRetryDelay = 3000; // 3 saniye başlangıç bekleme süresi

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Ücretsiz hesaplar için: Önce v1beta, sonra v1 API versiyonunu dene
                // Google AI Studio ücretsiz tier'ı için v1beta genellikle çalışır
                var apiVersions = new[] { "v1beta", "v1" };
                HttpResponseMessage? response = null;
                string? lastError = null;
                bool success = false;
                
                foreach (var apiVersion in apiVersions)
                {
                    try
                    {
                        var url = $"https://generativelanguage.googleapis.com/{apiVersion}/models/{_options.Model}:generateContent?key={_options.ApiKey}";
                        response = await _httpClient.PostAsync(url, content);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            // Başarılı API versiyonunu bulduk
                            success = true;
                            break;
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            lastError = errorContent;
                            // Sonraki API versiyonunu dene
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        // Sonraki API versiyonunu dene
                        continue;
                    }
                }
                
                // Hiçbir API versiyonu çalışmadıysa hata kontrolü yap
                if (!success || response == null || !response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrEmpty(lastError))
                    {
                        try
                        {
                            var errorJson = JsonDocument.Parse(lastError);
                            if (errorJson.RootElement.TryGetProperty("error", out var errorElement))
                            {
                                var errorMessage = errorElement.TryGetProperty("message", out var msgElement)
                                    ? msgElement.GetString()
                                    : errorElement.GetRawText();
                                
                                // Quota hatası kontrolü
                                if (!string.IsNullOrEmpty(errorMessage) && 
                                    (errorMessage.Contains("quota", StringComparison.OrdinalIgnoreCase) || 
                                     errorMessage.Contains("Quota exceeded", StringComparison.OrdinalIgnoreCase)))
                                {
                                    var retryTimeMatch = System.Text.RegularExpressions.Regex.Match(errorMessage, @"retry in (\d+\.?\d*)s", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    if (retryTimeMatch.Success && retryTimeMatch.Groups.Count > 1 && 
                                        double.TryParse(retryTimeMatch.Groups[1].Value, out var retrySeconds))
                                    {
                                        if (attempt < maxRetries)
                                        {
                                            await Task.Delay((int)(retrySeconds * 1000) + 1000);
                                            continue;
                                        }
                                    }
                                    return "Üzgünüm, şu anda yapay zekâ servisinin kullanım limiti aşılmış. Lütfen birkaç dakika sonra tekrar deneyin. (Quota limiti aşıldı)";
                                }
                                
                                // Rate limit hatası (429)
                                if (!string.IsNullOrEmpty(errorMessage) && 
                                    (errorMessage.Contains("rate limit", StringComparison.OrdinalIgnoreCase) || 
                                     errorMessage.Contains("429", StringComparison.OrdinalIgnoreCase) ||
                                     errorMessage.Contains("too many requests", StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (attempt < maxRetries)
                                    {
                                        var waitTime = baseRetryDelay * (int)Math.Pow(2, attempt - 1);
                                        await Task.Delay(waitTime);
                                        continue;
                                    }
                                    return "Çok fazla istek gönderildi. Lütfen 1-2 dakika bekleyip tekrar deneyin. (Rate limit aşıldı)";
                                }
                                
                                // API anahtarı süresi dolmuş hatası
                                if (!string.IsNullOrEmpty(errorMessage) && 
                                    (errorMessage.Contains("expired", StringComparison.OrdinalIgnoreCase) ||
                                     errorMessage.Contains("API key expired", StringComparison.OrdinalIgnoreCase) ||
                                     errorMessage.Contains("invalid API key", StringComparison.OrdinalIgnoreCase) ||
                                     errorMessage.Contains("API key not valid", StringComparison.OrdinalIgnoreCase)))
                                {
                                    return "❌ API anahtarı süresi dolmuş veya geçersiz. Lütfen Google AI Studio'dan (https://aistudio.google.com/) yeni bir API anahtarı oluşturun ve appsettings.json dosyasına ekleyin.";
                                }
                                
                                // Model bulunamadı hatası için özel mesaj
                                if (!string.IsNullOrEmpty(errorMessage) && errorMessage.Contains("not found"))
                                {
                                    // Son denemede değilse tekrar dene
                                    if (attempt < maxRetries)
                                    {
                                        await Task.Delay(baseRetryDelay * attempt);
                                        continue;
                                    }
                                    return "❌ Model bulunamadı. Lütfen Google AI Studio'dan (https://aistudio.google.com/) yeni bir API anahtarı oluşturun veya appsettings.json dosyasında 'Model' ayarını kontrol edin.";
                                }
                                
                                return $"❌ API hatası: {errorMessage}";
                            }
                        }
                        catch { }
                    }
                    
                    // Son denemede değilse tekrar dene
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(baseRetryDelay * attempt);
                        continue;
                    }
                    
                    return "API hatası: Gemini API'sine bağlanılamadı. Lütfen API anahtarınızı kontrol edin.";
                }
                
                // Başarılı yanıtı işle
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
            catch (HttpRequestException ex) when (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests"))
            {
                if (attempt < maxRetries)
                {
                    var waitTime = baseRetryDelay * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(waitTime);
                    continue;
                }
                return "Çok fazla istek gönderildi. Lütfen 1-2 dakika bekleyip tekrar deneyin. (Rate limit aşıldı)";
            }
            catch (Exception ex)
            {
                if (attempt < maxRetries)
                {
                    var waitTime = baseRetryDelay * attempt;
                    await Task.Delay(waitTime);
                    continue;
                }
                return $"Bir hata oluştu: {ex.Message}";
            }
        }

        // Tüm denemeler başarısız oldu
        return "Yapay zekâ servisine bağlanılamadı. Lütfen daha sonra tekrar deneyin.";
    }
}
