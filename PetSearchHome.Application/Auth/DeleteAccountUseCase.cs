using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record DeleteAccountRequest(string CurrentPassword);

    public class DeleteAccountUseCase : IUseCase<DeleteAccountRequest, Result<bool>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public DeleteAccountUseCase(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<Result<bool>> ExecuteAsync(DeleteAccountRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
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

            await _users.DeleteAsync(user.Id, cancellationToken);
            return Result.Success(true);
        }
    }
}
