using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Profiles
{
    public sealed record ViewProfileRequest(Guid UserId);

    public class ViewProfileUseCase : IUseCase<ViewProfileRequest, User?>
    {
        private readonly IUserRepository _users;

        public ViewProfileUseCase(IUserRepository users)
        {
            _users = users;
        }

        public Task<User?> ExecuteAsync(ViewProfileRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            return _users.GetByIdAsync(request.UserId, cancellationToken);
        }
    }
}
