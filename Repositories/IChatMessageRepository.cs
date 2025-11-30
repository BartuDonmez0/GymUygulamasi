using GymApp.Entities;

namespace GymApp.Repositories;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<IEnumerable<ChatMessage>> GetByMemberIdAsync(int memberId);
    Task<IEnumerable<ChatMessage>> GetAllWithMembersAsync();
}

