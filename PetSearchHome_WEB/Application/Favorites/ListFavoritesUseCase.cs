using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Favorites
{
    public sealed record ListFavoritesRequest();

    public class ListFavoritesUseCase : IUseCase<ListFavoritesRequest, Result<IReadOnlyList<PetListing>>>
    {
        private readonly IFavoriteRepository _favorites;
        private readonly IListingRepository _listings;

        public ListFavoritesUseCase(IFavoriteRepository favorites, IListingRepository listings)
        {
            _favorites = favorites;
            _listings = listings;
        }

        public async Task<Result<IReadOnlyList<PetListing>>> ExecuteAsync(ListFavoritesRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId == null)
            {
                return Result.Failure<IReadOnlyList<PetListing>>("Необхідна авторизація для перегляду улюблених.");
            }

            var favoriteIds = await _favorites.ListIdsByUserAsync(authContext.UserId.Value, cancellationToken);

            List<PetListing> listings = new();
            foreach (var id in favoriteIds)
            {
                var listing = await _listings.GetByIdAsync(id, cancellationToken);
                if (listing != null)
                {
                    listings.Add(listing);
                }
            }

            return Result.Success<IReadOnlyList<PetListing>>(listings);
        }
    }
}