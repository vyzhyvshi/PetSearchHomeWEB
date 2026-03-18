using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record ResetPasswordRequest(Guid UserId, string NewPassword);

    public class ResetPasswordUseCase : IUseCase<ResetPasswordRequest, bool>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public ResetPasswordUseCase(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<bool> ExecuteAsync(ResetPasswordRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.Role != Role.Admin && authContext.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Cannot reset password for another user.");
            }

            var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            await _users.UpdatePasswordAsync(user.Id, _hasher.Hash(request.NewPassword), cancellationToken);
            return true;
        }
    }
}
