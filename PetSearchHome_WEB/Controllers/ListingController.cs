using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Models.Listing;
using PetSearchHome_WEB.Security;

namespace PetSearchHome_WEB.Controllers
{
    [Authorize(Roles = RoleNames.AuthenticatedUser)]
    public class ListingController : AppController
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
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "\u0421\u0442\u0430\u043B\u0430\u0441\u044F \u043F\u043E\u043C\u0438\u043B\u043A\u0430 \u043F\u0440\u0438 \u0441\u0442\u0432\u043E\u0440\u0435\u043D\u043D\u0456 \u043E\u0433\u043E\u043B\u043E\u0448\u0435\u043D\u043D\u044F.");
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
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "\u041D\u0435 \u0432\u0434\u0430\u043B\u043E\u0441\u044F \u043E\u043D\u043E\u0432\u0438\u0442\u0438 \u043E\u0433\u043E\u043B\u043E\u0448\u0435\u043D\u043D\u044F.");
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
