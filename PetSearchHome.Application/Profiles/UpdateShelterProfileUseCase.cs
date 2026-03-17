using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Profiles
{
    public sealed record UpdateShelterProfileRequest(string DisplayName, string Description, string? Website, string? Address);

    public class UpdateShelterProfileUseCase : IUseCase<UpdateShelterProfileRequest, bool>
    {
        private readonly IShelterRepository _shelters;

        public UpdateShelterProfileUseCase(IShelterRepository shelters)
        {
            _shelters = shelters;
        }

        public async Task<bool> ExecuteAsync(UpdateShelterProfileRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!ShelterManagementPolicy.CanRunBulk(authContext.Role) || authContext.UserId is null)
            {
                throw new UnauthorizedAccessException("Shelter role required.");
            }

            var profile = new ShelterProfile
            {
                ShelterId = authContext.UserId.Value,
                DisplayName = request.DisplayName,
                Description = request.Description,
                Website = request.Website,
                Address = request.Address
            };

            await _shelters.UpsertProfileAsync(profile, cancellationToken);
            return true;
        }
    }
}
