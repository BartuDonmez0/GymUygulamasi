using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

// AIRecommendation tablosu için özel sorguları barındıran repository sınıfı.
public class AIRecommendationRepository : Repository<AIRecommendation>, IAIRecommendationRepository
{
    // Constructor - DbContext'i base repository'ye iletir.
    public AIRecommendationRepository(GymAppDbContext context) : base(context)
    {
    }

    // Belirli bir üyeye ait tüm AI önerilerini döndürür.
    public async Task<IEnumerable<AIRecommendation>> GetByMemberIdAsync(int memberId)
    {
        return await _dbSet
            .Where(r => r.MemberId == memberId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    // Öneri tipine göre (egzersiz, diyet vb.) AI önerilerini döndürür.
    public async Task<IEnumerable<AIRecommendation>> GetByTypeAsync(string recommendationType)
    {
        return await _dbSet
            .Where(r => r.RecommendationType == recommendationType)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    // Hem üye hem de öneri tipine göre filtrelenmiş önerileri döndürür.
    public async Task<IEnumerable<AIRecommendation>> GetByMemberIdAndTypeAsync(int memberId, string recommendationType)
    {
        return await _dbSet
            .Where(r => r.MemberId == memberId && r.RecommendationType == recommendationType)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }
}

