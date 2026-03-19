using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Favorites
{
    public sealed record ListFavoritesRequest();

    public class ListFavoritesUseCase : IUseCase<ListFavoritesRequest, IReadOnlyList<PetListing>>
    {
        private readonly IFavoriteRepository _favorites;
        private readonly IListingRepository _listings;

        public ListFavoritesUseCase(IFavoriteRepository favorites, IListingRepository listings)
        {
            _favorites = favorites;
            _listings = listings;
        }

        public async Task<IReadOnlyList<PetListing>> ExecuteAsync(ListFavoritesRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null) throw new UnauthorizedAccessException();

            var favoriteIds = await _favorites.ListIdsByUserAsync(authContext.UserId.Value, cancellationToken);
            return await _listings.GetByIdsAsync(favoriteIds, cancellationToken);
        }
    }
}