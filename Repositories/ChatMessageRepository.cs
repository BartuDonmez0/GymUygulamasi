using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(GymAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChatMessage>> GetByMemberIdAsync(int memberId)
    {
        return await _dbSet
            .Include(cm => cm.Member)
            .Where(cm => cm.MemberId == memberId)
            .OrderBy(cm => cm.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetAllWithMembersAsync()
    {
        return await _dbSet
            .Include(cm => cm.Member)
            .OrderByDescending(cm => cm.CreatedDate)
            .ToListAsync();
    }
}

