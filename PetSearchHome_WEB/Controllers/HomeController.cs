using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Favorites;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Models.Listing;
using System.Security.Claims;

namespace PetSearchHome_WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SearchAnimalsUseCase _searchAnimalsUseCase;
        private readonly ViewListingDetailUseCase _viewListingDetailUseCase;
        private readonly ToggleFavoriteUseCase _toggleFavoriteUseCase;
        private readonly SubmitComplaintUseCase _submitComplaintUseCase;
        private readonly IFavoriteRepository _favorites;

        public HomeController(
            ILogger<HomeController> logger,
            SearchAnimalsUseCase searchAnimalsUseCase,
            ViewListingDetailUseCase viewListingDetailUseCase,
            ToggleFavoriteUseCase toggleFavoriteUseCase,
            SubmitComplaintUseCase submitComplaintUseCase,
            IFavoriteRepository favorites)
        {
            _logger = logger;
            _searchAnimalsUseCase = searchAnimalsUseCase;
            _viewListingDetailUseCase = viewListingDetailUseCase;
            _toggleFavoriteUseCase = toggleFavoriteUseCase;
            _submitComplaintUseCase = submitComplaintUseCase;
            _favorites = favorites;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ListingFilterViewModel filter, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            _logger.LogInformation(
                "Catalog accessed by user {UserId}. Query: {SearchQuery}, Location: {Location}",
                authContext.UserId,
                filter.SearchQuery,
                filter.Location);

            try
            {
                var domainFilters = new SearchFilters
                {
                    SearchQuery = filter.SearchQuery,
                    AnimalType = filter.AnimalType,
                    Location = filter.Location,
                    IsUrgent = filter.OnlyUrgent ? true : null
                };

                var request = new SearchAnimalsRequest(domainFilters);
                var listings = await _searchAnimalsUseCase.ExecuteAsync(request, authContext, cancellationToken);

                var viewModel = new CatalogViewModel
                {
                    Filter = filter,
                    Results = listings.Select(l => new ListingSummaryViewModel
                    {
                        Id = l.Id,
                        Title = l.Title,
                        AnimalType = l.AnimalType,
                        Location = l.Location,
                        ListedAt = l.ListedAt,
                        IsUrgent = l.IsUrgent
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching animals.");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            try
            {
                var request = new ViewListingDetailRequest(id);
                var listing = await _viewListingDetailUseCase.ExecuteAsync(request, authContext, cancellationToken);
                if (listing is null)
                {
                    _logger.LogWarning("Listing {ListingId} not found or access denied for user {UserId}.", id, authContext.UserId);
                    return NotFound();
                }

                var isFavorite = false;
                if (authContext.UserId.HasValue)
                {
                    isFavorite = await _favorites.GetAsync(authContext.UserId.Value, listing.Id, cancellationToken) is not null;
                }

                var model = new ListingDetailsViewModel
                {
                    Listing = listing,
                    IsFavorite = isFavorite
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching listing details for {ListingId}.", id);
                return View("Error");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            try
            {
                var isAdded = await _toggleFavoriteUseCase.ExecuteAsync(new ToggleFavoriteRequest(id), authContext, cancellationToken);
                TempData["SuccessMessage"] = isAdded
                    ? "Оголошення додано в улюблені."
                    : "Оголошення видалено з улюблених.";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to toggle favorite for listing {ListingId}.", id);
                TempData["ErrorMessage"] = "Не вдалося оновити улюблені.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportListing(Guid id, string reason, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            try
            {
                var request = new SubmitComplaintRequest(id, reason);
                await _submitComplaintUseCase.ExecuteAsync(request, authContext, cancellationToken);
                TempData["SuccessMessage"] = "Скаргу успішно надіслано.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while submitting complaint for listing {ListingId}.", id);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit complaint for listing {ListingId}.", id);
                TempData["ErrorMessage"] = "Не вдалося надіслати скаргу.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private AuthContext GetAuthContext()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                return new AuthContext
                {
                    UserId = null,
                    Role = Role.Guest
                };
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdString, out Guid userId);
            var roleString = User.FindFirstValue(ClaimTypes.Role);

            Role userRole = Role.Person;
            if (!string.IsNullOrEmpty(roleString) && Enum.TryParse<Role>(roleString, true, out var parsedRole))
            {
                userRole = parsedRole;
            }

            return new AuthContext
            {
                UserId = userId,
                Role = userRole
            };
        }
    }
}
