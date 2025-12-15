using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

// ChatMessage tablosu için özel sorguları barındıran repository sınıfı.
public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
{
    // Constructor - DbContext'i base repository'ye iletir.
    public ChatMessageRepository(GymAppDbContext context) : base(context)
    {
    }

    // Belirli bir üyeye ait sohbet mesajlarını, üye bilgisi ile birlikte döndürür.
    public async Task<IEnumerable<ChatMessage>> GetByMemberIdAsync(int memberId)
    {
        return await _dbSet
            .Include(cm => cm.Member)
            .Where(cm => cm.MemberId == memberId)
            .OrderBy(cm => cm.CreatedDate)
            .ToListAsync();
    }

    // Tüm sohbet mesajlarını üye bilgisi ile birlikte döndürür.
    public async Task<IEnumerable<ChatMessage>> GetAllWithMembersAsync()
    {
        return await _dbSet
            .Include(cm => cm.Member)
            .OrderByDescending(cm => cm.CreatedDate)
            .ToListAsync();
    }
}

