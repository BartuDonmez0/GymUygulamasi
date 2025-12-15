using GymApp.Entities;

namespace GymApp.Services;

// Sohbet mesajı servisleri için CRUD imzalarını tanımlar.
public interface IChatMessageService
{
    Task<IEnumerable<ChatMessage>> GetChatMessagesByMemberIdAsync(int memberId);
    Task<IEnumerable<ChatMessage>> GetAllChatMessagesAsync();
    Task<ChatMessage> CreateChatMessageAsync(int memberId, string message, string? response = null);
    Task<ChatMessage> UpdateChatMessageResponseAsync(int messageId, string response);
    Task DeleteChatMessagesByMemberIdAsync(int memberId);
}

