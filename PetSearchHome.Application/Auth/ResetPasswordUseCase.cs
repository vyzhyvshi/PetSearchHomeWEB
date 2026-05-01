using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);

    public class ResetPasswordUseCase : IUseCase<ResetPasswordRequest, Result<bool>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly IPasswordResetTokenRepository _resetTokens;

        public ResetPasswordUseCase(IUserRepository users, IPasswordHasher hasher, IPasswordResetTokenRepository resetTokens)
        {
            _users = users;
            _hasher = hasher;
            _resetTokens = resetTokens;
        }

        public async Task<Result<bool>> ExecuteAsync(ResetPasswordRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                return Result.Failure<bool>("User not found.");
            }

            var token = await _resetTokens.GetUsableAsync(user.Id, RequestPasswordResetUseCase.HashToken(request.Token), cancellationToken);
            if (token is null)
            {
                return Result.Failure<bool>("Password reset token is invalid or expired.");
            }

            await _users.UpdatePasswordAsync(user.Id, _hasher.Hash(request.NewPassword), cancellationToken);
            await _resetTokens.MarkUsedAsync(token.Id, cancellationToken);

            return Result.Success(true);
        }
    }
}
