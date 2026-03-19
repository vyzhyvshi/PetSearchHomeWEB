using PetSearchHome_WEB.Application.Shared;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record LogoutRequest(Guid UserId);

    public class LogoutUseCase : IUseCase<LogoutRequest, bool>
    {
        public Task<bool> ExecuteAsync(LogoutRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Cannot logout other user.");
            }

            return Task.FromResult(true);
        }
    }
}
