using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Profiles
{
    public sealed record UpdateProfileRequest(Guid UserId, string DisplayName);

    public class UpdateProfileUseCase : IUseCase<UpdateProfileRequest, bool>
    {
        private readonly IUserRepository _users;

        public UpdateProfileUseCase(IUserRepository users)
        {
            _users = users;
        }

        public async Task<bool> ExecuteAsync(UpdateProfileRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId != request.UserId && authContext.Role != Role.Admin)
            {
                throw new UnauthorizedAccessException("Cannot update another profile.");
            }

            var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            var updated = new User
            {
                Id = user.Id,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                DisplayName = request.DisplayName,
                IsBlocked = user.IsBlocked
            };

            await _users.UpdateProfileAsync(updated, cancellationToken);
            return true;
        }
    }
}
