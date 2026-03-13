using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IShelterRepository
    {
        Task<ShelterProfile?> GetProfileAsync(Guid shelterId, CancellationToken cancellationToken = default);
        Task UpsertProfileAsync(ShelterProfile profile, CancellationToken cancellationToken = default);
    }
}
