using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Catalog
{
    public sealed record SearchAnimalsRequest(SearchFilters Filters);

    public class SearchAnimalsUseCase : IUseCase<SearchAnimalsRequest, Result<IReadOnlyList<PetListing>>>
    {
        private readonly ISearchGateway _searchGateway;

        public SearchAnimalsUseCase(ISearchGateway searchGateway)
        {
            _searchGateway = searchGateway;
        }

        public async Task<Result<IReadOnlyList<PetListing>>> ExecuteAsync(SearchAnimalsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var result = await _searchGateway.SearchAsync(request.Filters, cancellationToken);
            return Result.Success(result);
        }
    }
}