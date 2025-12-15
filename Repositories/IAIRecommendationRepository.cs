using GymApp.Entities;

namespace GymApp.Repositories;

// AIRecommendation için özel sorgu imzalarını tanımlar.
public interface IAIRecommendationRepository : IRepository<AIRecommendation>
{
    Task<IEnumerable<AIRecommendation>> GetByMemberIdAsync(int memberId);
    Task<IEnumerable<AIRecommendation>> GetByTypeAsync(string recommendationType);
    Task<IEnumerable<AIRecommendation>> GetByMemberIdAndTypeAsync(int memberId, string recommendationType);
}

