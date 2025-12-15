using GymApp.Entities;

namespace GymApp.Repositories;

// ChatMessage için özel sorgu imzalarını tanımlar.
public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<IEnumerable<ChatMessage>> GetByMemberIdAsync(int memberId);
    Task<IEnumerable<ChatMessage>> GetAllWithMembersAsync();
}

