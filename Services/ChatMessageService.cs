using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

// Üyelerin AI ile sohbet mesajlarını yöneten servis.
public class ChatMessageService : IChatMessageService
{
    private readonly IChatMessageRepository _chatMessageRepository;

    // Constructor - chat message repository bağımlılığını alır.
    public ChatMessageService(IChatMessageRepository chatMessageRepository)
    {
        _chatMessageRepository = chatMessageRepository;
    }

    // Belirli bir üyeye ait sohbet mesajlarını döndürür.
    public async Task<IEnumerable<ChatMessage>> GetChatMessagesByMemberIdAsync(int memberId)
    {
        return await _chatMessageRepository.GetByMemberIdAsync(memberId);
    }

    // Tüm üyelerin sohbet mesajlarını, üye bilgileriyle birlikte döndürür.
    public async Task<IEnumerable<ChatMessage>> GetAllChatMessagesAsync()
    {
        return await _chatMessageRepository.GetAllWithMembersAsync();
    }

    // Yeni bir sohbet mesajı oluşturur (opsiyonel AI cevabı ile).
    public async Task<ChatMessage> CreateChatMessageAsync(int memberId, string message, string? response = null)
    {
        var chatMessage = new ChatMessage
        {
            MemberId = memberId,
            Message = message,
            Response = response,
            CreatedDate = DateTime.UtcNow
        };

        return await _chatMessageRepository.AddAsync(chatMessage);
    }

    // Var olan bir sohbet mesajının AI cevabını günceller.
    public async Task<ChatMessage> UpdateChatMessageResponseAsync(int messageId, string response)
    {
        var chatMessage = await _chatMessageRepository.GetByIdAsync(messageId);
        if (chatMessage == null)
        {
            throw new ArgumentException("Chat message not found");
        }

        chatMessage.Response = response;
        await _chatMessageRepository.UpdateAsync(chatMessage);
        return chatMessage;
    }

    // Belirli bir üyeye ait tüm sohbet mesajlarını siler.
    public async Task DeleteChatMessagesByMemberIdAsync(int memberId)
    {
        var messages = await _chatMessageRepository.GetByMemberIdAsync(memberId);
        foreach (var message in messages)
        {
            await _chatMessageRepository.DeleteAsync(message);
        }
    }
}

