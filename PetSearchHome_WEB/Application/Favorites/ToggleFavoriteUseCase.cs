using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Favorites
{
    public sealed record ToggleFavoriteRequest(Guid ListingId);

    public class ToggleFavoriteUseCase : IUseCase<ToggleFavoriteRequest, Result<bool>>
    {
        private readonly IFavoriteRepository _favorites;

        public ToggleFavoriteUseCase(IFavoriteRepository favorites)
        {
            _favorites = favorites;
        }

        public async Task<Result<bool>> ExecuteAsync(ToggleFavoriteRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<bool>("Необхідна авторизація.");
            }

            var favorite = await _favorites.GetAsync(authContext.UserId.Value, request.ListingId, cancellationToken);

            if (favorite != null)
            {
                await _favorites.RemoveAsync(authContext.UserId.Value, request.ListingId, cancellationToken);
                return false; 
            }

            Favorite newFavorite = new()
            {
                UserId = authContext.UserId.Value,
                ListingId = request.ListingId
            };

            await _favorites.AddAsync(newFavorite, cancellationToken);

            return true;
        }
    }
}
