using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Favorites
{
    public sealed record ToggleFavoriteRequest(Guid ListingId);

    public class ToggleFavoriteUseCase : IUseCase<ToggleFavoriteRequest, bool>
    {
        private readonly IFavoriteRepository _favorites;

        public ToggleFavoriteUseCase(IFavoriteRepository favorites)
        {
            _favorites = favorites;
        }

        public async Task<bool> ExecuteAsync(ToggleFavoriteRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null) throw new UnauthorizedAccessException();

            var favorite = await _favorites.GetAsync(authContext.UserId.Value, request.ListingId, cancellationToken);

            if (favorite != null)
            {
                await _favorites.RemoveAsync(authContext.UserId.Value, request.ListingId, cancellationToken);
                return false; // Видалено
            }

            await _favorites.AddAsync(new Favorite
            {
                UserId = authContext.UserId.Value,
                ListingId = request.ListingId
            }, cancellationToken);

            return true; // Додано
        }
    }
}