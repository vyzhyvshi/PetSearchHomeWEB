using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Models.Listing;
using System.Security.Claims;

namespace PetSearchHome_WEB.Controllers
{
    [Authorize]
    public class ListingController : Controller
    {
        private readonly ILogger<ListingController> _logger;
        private readonly CreateListingUseCase _createListingUseCase;
        private readonly ListMyListingsUseCase _listMyListingsUseCase;
        private readonly DeleteListingUseCase _deleteListingUseCase;
        private readonly EditListingUseCase _editListingUseCase;
        private readonly SubmitListingForModerationUseCase _submitListingForModerationUseCase;
        private readonly ViewListingDetailUseCase _viewListingDetailUseCase;

        public ListingController(
            ILogger<ListingController> logger,
            CreateListingUseCase createListingUseCase,
            ListMyListingsUseCase listMyListingsUseCase,
            DeleteListingUseCase deleteListingUseCase,
            EditListingUseCase editListingUseCase,
            SubmitListingForModerationUseCase submitListingForModerationUseCase,
            ViewListingDetailUseCase viewListingDetailUseCase)
        {
            _logger = logger;
            _createListingUseCase = createListingUseCase;
            _listMyListingsUseCase = listMyListingsUseCase;
            _deleteListingUseCase = deleteListingUseCase;
            _editListingUseCase = editListingUseCase;
            _submitListingForModerationUseCase = submitListingForModerationUseCase;
            _viewListingDetailUseCase = viewListingDetailUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> MyListings(CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            _logger.LogInformation("User {UserId} requested their listings.", authContext.UserId);

            ListMyListingsRequest request = new();
            var result = await _listMyListingsUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed access to MyListings by user {UserId}: {Error}", authContext.UserId, result.ErrorMessage);
                return Forbid();
            }

            return View(result.Value);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateListingViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateListingViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authContext = GetAuthContext();

            CreateListingRequest request = new(
                model.Title,
                model.AnimalType,
                model.Location,
                model.Description,
                model.IsUrgent
            );

            var result = await _createListingUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("User {UserId} failed to create a listing: {Error}", authContext.UserId, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Сталася помилка при створенні оголошення.");
                return View(model);
            }

            _logger.LogInformation("Listing {ListingId} successfully created by user {UserId}", result.Value, authContext.UserId);
            return RedirectToAction(nameof(MyListings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            DeleteListingRequest request = new(id);
            var result = await _deleteListingUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to delete listing {ListingId} by user {UserId}: {Error}", id, authContext.UserId, result.ErrorMessage);
                // Можемо використати TempData щоб показати помилку на UI, або повернути NotFound/Forbid залежно від логіки.
                return NotFound();
            }

            _logger.LogInformation("Listing {ListingId} deleted by user {UserId}", id, authContext.UserId);
            return RedirectToAction(nameof(MyListings));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            ViewListingDetailRequest request = new(id);
            var result = await _viewListingDetailUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("Listing {ListingId} not found for edit form.", id);
                return NotFound();
            }

            var listing = result.Value;
            EditListingViewModel model = new()
            {
                Id = listing.Id,
                Title = listing.Title,
                AnimalType = listing.AnimalType,
                Location = listing.Location,
                Description = listing.Description,
                IsUrgent = listing.IsUrgent
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditListingViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authContext = GetAuthContext();

            EditListingRequest request = new(
                model.Id,
                model.Title,
                model.AnimalType,
                model.Location,
                model.Description,
                model.IsUrgent
            );

            var result = await _editListingUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to edit listing {ListingId} by user {UserId}: {Error}", model.Id, authContext.UserId, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не вдалося оновити оголошення.");
                return View(model);
            }

            _logger.LogInformation("Listing {ListingId} edited by user {UserId}", model.Id, authContext.UserId);
            return RedirectToAction(nameof(MyListings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForModeration(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            SubmitListingForModerationRequest request = new(id);
            var result = await _submitListingForModerationUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to submit listing {ListingId} for moderation by user {UserId}: {Error}", id, authContext.UserId, result.ErrorMessage);
                return NotFound();
            }

            _logger.LogInformation("Listing {ListingId} submitted for moderation by user {UserId}", id, authContext.UserId);
            return RedirectToAction(nameof(MyListings));
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
    }
}