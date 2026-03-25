using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record LoginRequest(string Email, string Password);
    public sealed record LoginResponse(string Token, Guid UserId, Domain.ValueObjects.Role Role);

    public class LoginUseCase : IUseCase<LoginRequest, LoginResponse>
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

        public async Task<LoginResponse> ExecuteAsync(LoginRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken)
                ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!_hasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var token = _tokens.IssueToken(user);
            return new LoginResponse(token, user.Id, user.Role);
        }
    }
}
