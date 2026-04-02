using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Favorites;
using PetSearchHome_WEB.Models.Favorite;
using PetSearchHome_WEB.Models.Listing;

namespace PetSearchHome_WEB.Controllers
{
    [Authorize]
    public class FavoriteController : AppController
    {
        private readonly ILogger<FavoriteController> _logger;
        private readonly ToggleFavoriteUseCase _toggleFavoriteUseCase;
        private readonly ListFavoritesUseCase _listFavoritesUseCase;

        public FavoriteController(
            ILogger<FavoriteController> logger,
            ToggleFavoriteUseCase toggleFavoriteUseCase,
            ListFavoritesUseCase listFavoritesUseCase)
        {
            _logger = logger;
            _toggleFavoriteUseCase = toggleFavoriteUseCase;
            _listFavoritesUseCase = listFavoritesUseCase;
        }

        // GET: /Favorite/Index
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            _logger.LogInformation("User {UserId} viewing favorites.", authContext.UserId);

            ListFavoritesRequest request = new();
            var result = await _listFavoritesUseCase.ExecuteAsync(request, authContext, cancellationToken);

            var listings = result.IsSuccess && result.Value != null ? result.Value : new List<Domain.Entities.PetListing>();

            FavoriteViewModel viewModel = new()
            {
                Items = listings.Select(l => new ListingSummaryViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    AnimalType = l.AnimalType,
                    Location = l.Location,
                    IsUrgent = l.IsUrgent,
                    ListedAt = l.ListedAt,
                    PhotoUrl = l.PrimaryPhotoUrl
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Favorite/Toggle/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            ToggleFavoriteRequest request = new(id);
            var result = await _toggleFavoriteUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Error toggling favorite for listing {ListingId}: {Error}", id, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            var isAdded = result.Value;

            _logger.LogInformation("Listing {ListingId} toggle favorite for user {UserId}. Status: {Status}",
                id, authContext.UserId, isAdded ? "Added" : "Removed");

            return Ok(new { added = isAdded });
        }

    }
}
