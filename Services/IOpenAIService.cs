namespace GymApp.Services;

public interface IOpenAIService
{
    Task<string> GetChatResponseAsync(string userMessage, string? userContext = null);
    Task<string> GetExerciseRecommendationAsync(string bodyType, double? height = null, double? weight = null, string? photoDescription = null);
    Task<string> GetDietRecommendationAsync(string bodyType, double? height = null, double? weight = null, string? photoDescription = null);
    Task<string> GetVisualizationAsync(string exercisePlan, string bodyType);
}

