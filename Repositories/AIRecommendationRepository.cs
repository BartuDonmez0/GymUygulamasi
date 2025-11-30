using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

public class AIRecommendationRepository : Repository<AIRecommendation>, IAIRecommendationRepository
{
    public AIRecommendationRepository(GymAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AIRecommendation>> GetByMemberIdAsync(int memberId)
    {
        return await _dbSet
            .Where(r => r.MemberId == memberId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AIRecommendation>> GetByTypeAsync(string recommendationType)
    {
        return await _dbSet
            .Where(r => r.RecommendationType == recommendationType)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AIRecommendation>> GetByMemberIdAndTypeAsync(int memberId, string recommendationType)
    {
        return await _dbSet
            .Where(r => r.MemberId == memberId && r.RecommendationType == recommendationType)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }
}

