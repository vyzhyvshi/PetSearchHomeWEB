using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
        Task<PasswordResetToken?> GetUsableAsync(int userId, string tokenHash, CancellationToken cancellationToken = default);
        Task MarkUsedAsync(int tokenId, CancellationToken cancellationToken = default);
    }
}
