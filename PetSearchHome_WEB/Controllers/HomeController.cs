using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Favorites;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Models;
using PetSearchHome_WEB.Models.Listing;
using System.Diagnostics;
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
                authContext.UserId, filter.SearchQuery, filter.Location);

            SearchFilters domainFilters = new()
            {
                SearchQuery = filter.SearchQuery,
                AnimalType = filter.AnimalType,
                Location = filter.Location,
                IsUrgent = filter.OnlyUrgent ? true : null
            };

            SearchAnimalsRequest request = new(domainFilters);
            var result = await _searchAnimalsUseCase.ExecuteAsync(request, authContext, cancellationToken);

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

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            ViewListingDetailRequest request = new(id);
            var result = await _viewListingDetailUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess || result.Value is null)
            {
                _logger.LogWarning("Listing {ListingId} not found or access denied for user {UserId}. Reason: {Reason}", id, authContext.UserId, result.ErrorMessage);
                return NotFound();
            }

            var listing = result.Value;
            var isFavorite = false;
            if (authContext.UserId.HasValue)
            {
                isFavorite = await _favorites.GetAsync(authContext.UserId.Value, listing.Id, cancellationToken) is not null;
            }

            ListingDetailsViewModel model = new()
            {
                Listing = listing,
                IsFavorite = isFavorite
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            ToggleFavoriteRequest request = new(id);
            var result = await _toggleFavoriteUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Value
                    ? "Оголошення додано в улюблені."
                    : "Оголошення видалено з улюблених.";
            }
            else
            {
                _logger.LogWarning("Failed to toggle favorite for listing {ListingId}: {Error}", id, result.ErrorMessage);
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося оновити улюблені.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportListing(Guid id, string reason, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            SubmitComplaintRequest request = new(id, reason);
            var result = await _submitComplaintUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Скаргу успішно надіслано.";
            }
            else
            {
                _logger.LogWarning("Validation error while submitting complaint for listing {ListingId}: {Error}", id, result.ErrorMessage);
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося надіслати скаргу.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private AuthContext GetAuthContext()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated) return new AuthContext { UserId = null, Role = Role.Guest };

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdString, out Guid userId);
            var roleString = User.FindFirstValue(ClaimTypes.Role);

            Role userRole = Role.Person;
            if (!string.IsNullOrEmpty(roleString) && Enum.TryParse<Role>(roleString, true, out var parsedRole))
            {
                userRole = parsedRole;
            }

            return new AuthContext { UserId = userId, Role = userRole };
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error,
                    "Глобальний обробник спіймав фатальну помилку на шляху: {Path}",
                    exceptionHandlerPathFeature.Path);
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}