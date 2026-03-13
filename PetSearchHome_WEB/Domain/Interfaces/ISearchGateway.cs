using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface ISearchGateway
    {
        Task<IReadOnlyList<PetListing>> SearchAsync(SearchFilters filters, CancellationToken cancellationToken = default);
    }
}
