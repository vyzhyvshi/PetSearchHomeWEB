using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Favorites;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Models;
using PetSearchHome_WEB.Models.Listing;
using System.Diagnostics;

namespace PetSearchHome_WEB.Controllers
{
    public class HomeController : AppController
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

            var listings = result.IsSuccess && result.Value != null
                ? result.Value
                : new List<PetSearchHome_WEB.Domain.Entities.PetListing>();

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
                    IsUrgent = l.IsUrgent,
                    PhotoUrl = l.PrimaryPhotoUrl
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
                SetSuccessMessage(result.Value
                    ? "\u041E\u0433\u043E\u043B\u043E\u0448\u0435\u043D\u043D\u044F \u0434\u043E\u0434\u0430\u043D\u043E \u0432 \u0443\u043B\u044E\u0431\u043B\u0435\u043D\u0456."
                    : "\u041E\u0433\u043E\u043B\u043E\u0448\u0435\u043D\u043D\u044F \u0432\u0438\u0434\u0430\u043B\u0435\u043D\u043E \u0437 \u0443\u043B\u044E\u0431\u043B\u0435\u043D\u0438\u0445.");
            }
            else
            {
                _logger.LogWarning("Failed to toggle favorite for listing {ListingId}: {Error}", id, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage ?? "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u043E\u043D\u043E\u0432\u0438\u0442\u0438 \u0443\u043B\u044E\u0431\u043B\u0435\u043D\u0456.");
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
                SetSuccessMessage("\u0421\u043A\u0430\u0440\u0433\u0443 \u0443\u0441\u043F\u0456\u0448\u043D\u043E \u043D\u0430\u0434\u0456\u0441\u043B\u0430\u043D\u043E.");
            }
            else
            {
                _logger.LogWarning("Validation error while submitting complaint for listing {ListingId}: {Error}", id, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage ?? "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u043D\u0430\u0434\u0456\u0441\u043B\u0430\u0442\u0438 \u0441\u043A\u0430\u0440\u0433\u0443.");
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error,
                    "\u0413\u043B\u043E\u0431\u0430\u043B\u044C\u043D\u0438\u0439 \u043E\u0431\u0440\u043E\u0431\u043D\u0438\u043A \u0441\u043F\u0456\u0439\u043C\u0430\u0432 \u0444\u0430\u0442\u0430\u043B\u044C\u043D\u0443 \u043F\u043E\u043C\u0438\u043B\u043A\u0443 \u043D\u0430 \u0448\u043B\u044F\u0445\u0443: {Path}",
                    exceptionHandlerPathFeature.Path);
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
