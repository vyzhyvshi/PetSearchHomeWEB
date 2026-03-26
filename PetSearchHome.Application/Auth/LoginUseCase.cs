using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record LoginRequest(string Email, string Password);
    public sealed record LoginResponse(string Token, Guid UserId, Domain.ValueObjects.Role Role);

    public class LoginUseCase : IUseCase<LoginRequest, Result<LoginResponse>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly IAuthTokenService _tokens;

        public LoginUseCase(IUserRepository users, IPasswordHasher hasher, IAuthTokenService tokens)
        {
            _users = users;
            _hasher = hasher;
            _tokens = tokens;
        }

        public async Task<Result<LoginResponse>> ExecuteAsync(LoginRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null || !_hasher.Verify(request.Password, user.PasswordHash))
            {
                return Result.Failure<LoginResponse>("Неправильний email або пароль.");
            }

            var token = _tokens.IssueToken(user);
            LoginResponse response = new(token, user.Id, user.Role);

            return Result.Success(response);
        }
    }
}