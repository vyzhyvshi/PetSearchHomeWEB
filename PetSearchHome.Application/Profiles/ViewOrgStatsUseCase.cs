using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Profiles
{
    public sealed record ViewOrgStatsRequest(Guid ShelterId);

    public class ViewOrgStatsUseCase : IUseCase<ViewOrgStatsRequest, OrgStats?>
    {
        private readonly IOrgStatsRepository _stats;

        public ViewOrgStatsUseCase(IOrgStatsRepository stats)
        {
            _stats = stats;
        }

        public Task<OrgStats?> ExecuteAsync(ViewOrgStatsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var canView = authContext.Role == Domain.ValueObjects.Role.Admin
                || (ShelterManagementPolicy.CanRunBulk(authContext.Role) && authContext.UserId == request.ShelterId);

            if (!canView)
            {
                throw new UnauthorizedAccessException("Not allowed to view stats.");
            }

            return _stats.GetAsync(request.ShelterId, cancellationToken);
        }
    }
}
