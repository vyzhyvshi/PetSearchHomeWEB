using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IModerationQueue
    {
        Task EnqueueAsync(PetListing listing, CancellationToken cancellationToken = default);
    }
}
