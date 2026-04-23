using Microsoft.Extensions.Options;
using PetSearchHome_WEB.Application.Shared;
using System.Linq;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Profiles
{
    public sealed record SearchPublicUsersWithListingsRequest(string Query);

    public class SearchPublicUsersWithListingsUseCase : IUseCase<SearchPublicUsersWithListingsRequest, Result<IReadOnlyList<User>>>
    {
        private readonly IUserRepository _users;
        private readonly IListingRepository _listings;
        private readonly SearchSettings _settings;

        public SearchPublicUsersWithListingsUseCase(
            IUserRepository users,
            IListingRepository listings,
            IOptions<SearchSettings> options)
        {
            _users = users;
            _listings = listings;
            _settings = options.Value;
        }

        public async Task<Result<IReadOnlyList<User>>> ExecuteAsync(SearchPublicUsersWithListingsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var query = request.Query?.Trim();
            if (string.IsNullOrWhiteSpace(query) || query.Length < _settings.MinQueryLength)
            {
                return Result.Success<IReadOnlyList<User>>(Array.Empty<User>());
            }

            var users = await _users.SearchAsync(query, cancellationToken);
            if (users.Count == 0)
            {
                return Result.Success<IReadOnlyList<User>>(Array.Empty<User>());
            }

            var tasks = users.Select(async user => new
            {
                User = user,
                Listings = await _listings.ListByOwnerAsync(user.Id, cancellationToken)
            });

            var results = await Task.WhenAll(tasks);
            var filtered = results
                .Where(result => result.Listings.Count > 0)
                .Select(result => result.User)
                .Take(_settings.MaxResults)
                .ToList();

            return Result.Success<IReadOnlyList<User>>(filtered);
        }
    }
}
