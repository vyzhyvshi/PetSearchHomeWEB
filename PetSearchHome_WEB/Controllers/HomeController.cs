using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Shared;
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

        public HomeController(
            ILogger<HomeController> logger,
            SearchAnimalsUseCase searchAnimalsUseCase,
            ViewListingDetailUseCase viewListingDetailUseCase)
        {
            _logger = logger;
            _searchAnimalsUseCase = searchAnimalsUseCase;
            _viewListingDetailUseCase = viewListingDetailUseCase;
        }

        // GET: / (Головна сторінка - Каталог тварин)
        [HttpGet]
        public async Task<IActionResult> Index(ListingFilterViewModel filter, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            _logger.LogInformation("Catalog accessed by user {UserId}. Query: {SearchQuery}, Location: {Location}",
                authContext.UserId, filter.SearchQuery, filter.Location);

            try
            {
                var domainFilters = new SearchFilters
                {
                    AnimalType = filter.AnimalType,
                    Location = filter.Location,
                    IsUrgent = filter.OnlyUrgent
                };

                var request = new SearchAnimalsRequest(domainFilters);
                var listings = await _searchAnimalsUseCase.ExecuteAsync(request, authContext, cancellationToken);

                var summaryList = listings.Select(l => new ListingSummaryViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    AnimalType = l.AnimalType,
                    Location = l.Location,
                    ListedAt = l.ListedAt,
                    IsUrgent = l.IsUrgent
                }).ToList();

                var viewModel = new CatalogViewModel
                {
                    Filter = filter, 
                    Results = summaryList
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching animals.");
                return View("Error"); 
            }
        }

        // GET: /Home/Details/{id} 
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

                _logger.LogInformation("Listing {ListingId} viewed by user {UserId}.", id, authContext.UserId);

                
                return View(listing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching listing details for {ListingId}.", id);
                return View("Error");
            }
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