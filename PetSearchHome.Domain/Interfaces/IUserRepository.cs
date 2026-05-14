using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task AddAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateRoleAsync(int id, Role role, CancellationToken cancellationToken = default);
        Task UpdatePasswordAsync(int id, string passwordHash, CancellationToken cancellationToken = default);
        Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default);
        Task SetBlockedAsync(int id, bool isBlocked, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> SearchAsync(string query, CancellationToken cancellationToken = default);
    }
}
