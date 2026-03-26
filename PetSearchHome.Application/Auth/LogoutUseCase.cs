using PetSearchHome_WEB.Application.Shared;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record LogoutRequest(Guid UserId);

    public class LogoutUseCase : IUseCase<LogoutRequest, Result<bool>>
    {
        public Task<Result<bool>> ExecuteAsync(LogoutRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId != request.UserId)
            {
                return Task.FromResult(Result.Failure<bool>("Cannot logout other user."));
            }

            return Task.FromResult(Result.Success(true));
        }
    }
}