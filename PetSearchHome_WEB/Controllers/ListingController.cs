using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
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
        private readonly IUserRepository _users;

        public ListingController(
            ILogger<ListingController> logger,
            CreateListingUseCase createListingUseCase,
            ListMyListingsUseCase listMyListingsUseCase,
            DeleteListingUseCase deleteListingUseCase,
            EditListingUseCase editListingUseCase,
            SubmitListingForModerationUseCase submitListingForModerationUseCase,
            ViewListingDetailUseCase viewListingDetailUseCase,
            IUserRepository users)
        {
            _logger = logger;
            _createListingUseCase = createListingUseCase;
            _listMyListingsUseCase = listMyListingsUseCase;
            _deleteListingUseCase = deleteListingUseCase;
            _editListingUseCase = editListingUseCase;
            _submitListingForModerationUseCase = submitListingForModerationUseCase;
            _viewListingDetailUseCase = viewListingDetailUseCase;
            _users = users;
        }

        [HttpGet]
        public Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return MyListings(cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> MyListings(CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            _logger.LogInformation("User {UserId} requested their listings.", authContext.UserId);

            var result = await _listMyListingsUseCase.ExecuteAsync(new ListMyListingsRequest(), authContext, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed access to MyListings by user {UserId}: {Error}", authContext.UserId, result.ErrorMessage);
                return Forbid();
            }

            return View("Index", result.Value);
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

            var authContext = await GetAuthContextAsync(cancellationToken);
            var request = new CreateListingRequest(
                model.Title,
                model.AnimalType,
                model.Location,
                model.Description,
                model.IsUrgent,
                ParsePhotoUrls(model.PhotoUrlsText));

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
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _deleteListingUseCase.ExecuteAsync(new DeleteListingRequest(id), authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to delete listing {ListingId} by user {UserId}: {Error}", id, authContext.UserId, result.ErrorMessage);
                return NotFound();
            }

            _logger.LogInformation("Listing {ListingId} deleted by user {UserId}", id, authContext.UserId);
            return RedirectToAction(nameof(MyListings));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _viewListingDetailUseCase.ExecuteAsync(new ViewListingDetailRequest(id), authContext, cancellationToken);

            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("Listing {ListingId} not found for edit form.", id);
                return NotFound();
            }

            var listing = result.Value;
            if (!ListingAccessPolicy.CanManage(authContext.Role, listing.OwnerId, authContext.UserId))
            {
                return Forbid();
            }

            var model = new EditListingViewModel
            {
                Id = listing.Id,
                Title = listing.Title,
                AnimalType = listing.AnimalType,
                Location = listing.Location,
                Description = listing.Description,
                IsUrgent = listing.IsUrgent,
                PhotoUrlsText = string.Join(Environment.NewLine, listing.PhotoUrls)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditListingViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authContext = await GetAuthContextAsync(cancellationToken);
            var request = new EditListingRequest(
                model.Id,
                model.Title,
                model.AnimalType,
                model.Location,
                model.Description,
                model.IsUrgent,
                ParsePhotoUrls(model.PhotoUrlsText));

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
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _submitListingForModerationUseCase.ExecuteAsync(new SubmitListingForModerationRequest(id), authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to submit listing {ListingId} for moderation by user {UserId}: {Error}", id, authContext.UserId, result.ErrorMessage);
                return NotFound();
            }

            _logger.LogInformation("Listing {ListingId} submitted for moderation by user {UserId}", id, authContext.UserId);
            return RedirectToAction(nameof(MyListings));
        }

        private async Task<AuthContext> GetAuthContextAsync(CancellationToken cancellationToken)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return new AuthContext { UserId = null, Role = Role.Guest };
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdString, out var userId);
            var roleString = User.FindFirstValue(ClaimTypes.Role);
            var email = User.FindFirstValue(ClaimTypes.Email);

            var role = Role.Person;
            if (!string.IsNullOrEmpty(roleString) && Enum.TryParse<Role>(roleString, true, out var parsedRole))
            {
                role = parsedRole;
            }

            if (userId != Guid.Empty)
            {
                var existing = await _users.GetByIdAsync(userId, cancellationToken);
                if (existing is not null)
                {
                    return new AuthContext { UserId = userId, Role = role };
                }
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await _users.GetByEmailAsync(email, cancellationToken);
                if (user is not null)
                {
                    return new AuthContext { UserId = user.Id, Role = user.Role };
                }
            }

            return new AuthContext { UserId = userId == Guid.Empty ? null : userId, Role = role };
        }

        private static IReadOnlyList<string> ParsePhotoUrls(string? photoUrlsText)
        {
            if (string.IsNullOrWhiteSpace(photoUrlsText))
            {
                return Array.Empty<string>();
            }

            return photoUrlsText
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(static url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
