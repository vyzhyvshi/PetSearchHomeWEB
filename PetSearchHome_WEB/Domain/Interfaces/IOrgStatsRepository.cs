using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IOrgStatsRepository
    {
        Task<OrgStats?> GetAsync(Guid shelterId, CancellationToken cancellationToken = default);
        Task UpsertAsync(OrgStats stats, CancellationToken cancellationToken = default);
    }
}
