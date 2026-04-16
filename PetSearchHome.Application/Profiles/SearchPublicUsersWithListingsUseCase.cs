using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Profiles
{
    public sealed record SearchPublicUsersWithListingsRequest(string Query);

    public class SearchPublicUsersWithListingsUseCase : IUseCase<SearchPublicUsersWithListingsRequest, Result<IReadOnlyList<User>>>
    {
        private readonly IUserRepository _users;
        private readonly IListingRepository _listings;

        public SearchPublicUsersWithListingsUseCase(IUserRepository users, IListingRepository listings)
        {
            _users = users;
            _listings = listings;
        }

        public async Task<Result<IReadOnlyList<User>>> ExecuteAsync(SearchPublicUsersWithListingsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Result.Success<IReadOnlyList<User>>(Array.Empty<User>());
            }

            var users = await _users.SearchAsync(request.Query, cancellationToken);
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
                .ToList();

            return Result.Success<IReadOnlyList<User>>(filtered);
        }
    }
}
