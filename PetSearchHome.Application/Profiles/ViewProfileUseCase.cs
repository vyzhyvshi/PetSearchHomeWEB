using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Profiles
{
    public sealed record ViewProfileRequest(Guid UserId);

    public class ViewProfileUseCase : IUseCase<ViewProfileRequest, Result<User>>
    {
        private readonly IUserRepository _users;

        public ViewProfileUseCase(IUserRepository users)
        {
            _users = users;
        }

        public async Task<Result<User>> ExecuteAsync(ViewProfileRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<User>("Профіль користувача не знайдено.");
            }

            return Result.Success(user);
        }
    }
}