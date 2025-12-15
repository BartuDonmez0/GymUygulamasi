using GymApp.Entities;

namespace GymApp.Repositories;

// Member için özel sorgu imzalarını tanımlar.
public interface IMemberRepository : IRepository<Member>
{
    Task<Member?> GetByEmailAsync(string email);
    Task<Member?> GetByEmailAndPasswordAsync(string email, string password);
    Task<Member?> GetWithAppointmentsAsync(int id);
    Task<Member?> GetWithRecommendationsAsync(int id);
}

