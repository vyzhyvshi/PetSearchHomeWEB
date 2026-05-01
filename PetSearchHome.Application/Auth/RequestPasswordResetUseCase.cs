using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record RequestPasswordResetRequest(string Email);

    public class RequestPasswordResetUseCase : IUseCase<RequestPasswordResetRequest, Result<string>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordResetTokenRepository _resetTokens;
        private readonly IEmailSender _emailSender;

        public RequestPasswordResetUseCase(
            IUserRepository users,
            IPasswordResetTokenRepository resetTokens,
            IEmailSender emailSender)
        {
            _users = users;
            _resetTokens = resetTokens;
            _emailSender = emailSender;
        }

        public async Task<Result<string>> ExecuteAsync(RequestPasswordResetRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                return Result.Failure<string>("Користувача не знайдено.");
            }

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            await _resetTokens.AddAsync(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = HashToken(token),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            }, cancellationToken);

            await _emailSender.SendPasswordResetAsync(user.Email, token, cancellationToken);
            return Result.Success(token);
        }

        internal static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}
