using Microsoft.Extensions.Options;
using System.Linq;
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
        private readonly SearchSettings _settings;

        public SearchAnimalsUseCase(ISearchGateway searchGateway, IOptions<SearchSettings> options)
        {
            _searchGateway = searchGateway;
            _settings = options.Value;
        }

        public async Task<Result<IReadOnlyList<PetListing>>> ExecuteAsync(SearchAnimalsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var searchQuery = request.Filters.SearchQuery?.Trim();
            if (!string.IsNullOrWhiteSpace(searchQuery) && searchQuery.Length < _settings.MinQueryLength)
            {
                searchQuery = null;
            }

            var normalizedFilters = new SearchFilters
            {
                SearchQuery = searchQuery?.ToLowerInvariant(),
                AnimalType = request.Filters.AnimalType?.ToLowerInvariant(),
                Location = request.Filters.Location?.ToLowerInvariant(),
                IsUrgent = request.Filters.IsUrgent
            };

            IReadOnlyList<PetListing> result = await _searchGateway.SearchAsync(normalizedFilters, cancellationToken);
            if (result.Count > _settings.MaxResults)
            {
                result = result.Take(_settings.MaxResults).ToList();
            }

            return Result.Success(result);
        }
    }
}
