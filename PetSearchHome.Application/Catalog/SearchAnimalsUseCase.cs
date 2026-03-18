using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Catalog
{
    public sealed record SearchAnimalsRequest(SearchFilters Filters);

    public class SearchAnimalsUseCase : IUseCase<SearchAnimalsRequest, IReadOnlyList<PetListing>>
    {
        private readonly ISearchGateway _searchGateway;

        public SearchAnimalsUseCase(ISearchGateway searchGateway)
        {
            _searchGateway = searchGateway;
        }

        public Task<IReadOnlyList<PetListing>> ExecuteAsync(SearchAnimalsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            // Guests are allowed; other roles too.
            return _searchGateway.SearchAsync(request.Filters, cancellationToken);
        }
    }
}
