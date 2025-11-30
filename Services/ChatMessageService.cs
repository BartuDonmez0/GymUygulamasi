using GymApp.Entities;
using GymApp.Repositories;

namespace GymApp.Services;

public class ChatMessageService : IChatMessageService
{
    private readonly IChatMessageRepository _chatMessageRepository;

    public ChatMessageService(IChatMessageRepository chatMessageRepository)
    {
        _chatMessageRepository = chatMessageRepository;
    }

    public async Task<IEnumerable<ChatMessage>> GetChatMessagesByMemberIdAsync(int memberId)
    {
        return await _chatMessageRepository.GetByMemberIdAsync(memberId);
    }

    public async Task<IEnumerable<ChatMessage>> GetAllChatMessagesAsync()
    {
        return await _chatMessageRepository.GetAllWithMembersAsync();
    }

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
}

