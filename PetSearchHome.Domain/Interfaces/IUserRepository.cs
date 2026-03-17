using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task AddAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateRoleAsync(Guid id, Role role, CancellationToken cancellationToken = default);
        Task UpdatePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default);
        Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default);
        Task SetBlockedAsync(Guid id, bool isBlocked, CancellationToken cancellationToken = default);
    }
}
