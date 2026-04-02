using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record RequestPasswordResetRequest(string Email);

    public class RequestPasswordResetUseCase : IUseCase<RequestPasswordResetRequest, Result<bool>>
    {
        private readonly IUserRepository _users;
        private readonly IAuthTokenService _tokens;
        private readonly IEmailSender _emailSender;

        public RequestPasswordResetUseCase(
            IUserRepository users,
            IAuthTokenService tokens,
            IEmailSender emailSender)
        {
            _users = users;
            _tokens = tokens;
            _emailSender = emailSender;
        }

        public async Task<Result<bool>> ExecuteAsync(RequestPasswordResetRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                return Result.Failure<bool>("Користувача не знайдено.");
            }

            var token = _tokens.IssueToken(user);
            await _emailSender.SendPasswordResetAsync(user.Email, token, cancellationToken);

            return true;
        }
    }
}
