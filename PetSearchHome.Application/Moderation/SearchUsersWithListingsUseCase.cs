using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record SearchUsersWithListingsRequest(string Query);

    public class SearchUsersWithListingsUseCase : IUseCase<SearchUsersWithListingsRequest, Result<IReadOnlyList<User>>>
    {
        private readonly IUserRepository _users;
        private readonly IListingRepository _listings;

        public SearchUsersWithListingsUseCase(IUserRepository users, IListingRepository listings)
        {
            _users = users;
            _listings = listings;
        }

        public async Task<Result<IReadOnlyList<User>>> ExecuteAsync(SearchUsersWithListingsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                return Result.Failure<IReadOnlyList<User>>("Немає прав доступу. Потрібна роль Адміністратора.");
            }

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
