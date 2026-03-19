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

            try
            {
                var request = new ListMyListingsRequest();
                var listings = await _listMyListingsUseCase.ExecuteAsync(request, authContext, cancellationToken);

                return View(listings);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to MyListings by user {UserId}", authContext.UserId);
                return Forbid();
            }
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

            try
            {
                var request = new CreateListingRequest(
                    model.Title,
                    model.AnimalType,
                    model.Location,
                    model.Description,
                    model.IsUrgent
                );

                var newListingId = await _createListingUseCase.ExecuteAsync(request, authContext, cancellationToken);

                _logger.LogInformation("Listing {ListingId} successfully created by user {UserId}", newListingId, authContext.UserId);

                return RedirectToAction(nameof(MyListings));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} lacked permissions to create a listing.", authContext.UserId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating listing for user {UserId}", authContext.UserId);
                ModelState.AddModelError(string.Empty, "Сталася помилка при створенні оголошення. Спробуйте пізніше.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            try
            {
                var request = new DeleteListingRequest(id);
                await _deleteListingUseCase.ExecuteAsync(request, authContext, cancellationToken);

                _logger.LogInformation("Listing {ListingId} deleted by user {UserId}", id, authContext.UserId);
                return RedirectToAction(nameof(MyListings));
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning(ex, "Listing {ListingId} not found for deletion.", id);
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} unauthorized to delete listing {ListingId}.", authContext.UserId, id);
                return Forbid();
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

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            var request = new ViewListingDetailRequest(id);
            var listing = await _viewListingDetailUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (listing == null)
            {
                _logger.LogWarning("Listing {ListingId} not found for edit form.", id);
                return NotFound();
            }

            var model = new EditListingViewModel
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

        // POST: /Listing/Edit/{id}
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

            try
            {
                var request = new EditListingRequest(
                    model.Id,
                    model.Title,
                    model.AnimalType,
                    model.Location,
                    model.Description,
                    model.IsUrgent
                );

                await _editListingUseCase.ExecuteAsync(request, authContext, cancellationToken);
                _logger.LogInformation("Listing {ListingId} edited by user {UserId}", model.Id, authContext.UserId);

                return RedirectToAction(nameof(MyListings));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Listing {ListingId} not found for edit.", model.Id);
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} unauthorized to edit listing {ListingId}.", authContext.UserId, model.Id);
                return Forbid();
            }
        }

        // POST: /Listing/SubmitForModeration/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForModeration(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            try
            {
                var request = new SubmitListingForModerationRequest(id);
                await _submitListingForModerationUseCase.ExecuteAsync(request, authContext, cancellationToken);

                _logger.LogInformation("Listing {ListingId} submitted for moderation by user {UserId}", id, authContext.UserId);
                return RedirectToAction(nameof(MyListings));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Listing {ListingId} not found for moderation submit.", id);
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} unauthorized to submit listing {ListingId} for moderation.", authContext.UserId, id);
                return Forbid();
            }
        }
    }
}