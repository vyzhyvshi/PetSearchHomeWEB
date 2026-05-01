using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    public class ChangePasswordUseCase : IUseCase<ChangePasswordRequest, Result<bool>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public ChangePasswordUseCase(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<Result<bool>> ExecuteAsync(ChangePasswordRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<bool>("Потрібна авторизація.");
            }

            var user = await _users.GetByIdAsync(authContext.UserId.Value, cancellationToken);
            if (user is null || !_hasher.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return Result.Failure<bool>("Поточний пароль неправильний.");
            }

            await _users.UpdatePasswordAsync(user.Id, _hasher.Hash(request.NewPassword), cancellationToken);
            return Result.Success(true);
        }
    }
}
