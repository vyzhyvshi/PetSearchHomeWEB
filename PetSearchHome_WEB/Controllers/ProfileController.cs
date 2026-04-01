using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Profiles;
using PetSearchHome_WEB.Application.Reviews;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Models.Profile;
using System.Security.Claims;

namespace PetSearchHome_WEB.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly ViewProfileUseCase _viewProfileUseCase;
        private readonly ViewProfileDetailsUseCase _viewProfileDetailsUseCase;
        private readonly UpdateProfileUseCase _updateProfileUseCase;
        private readonly UpdateShelterProfileUseCase _updateShelterProfileUseCase;
        private readonly ViewOrgStatsUseCase _viewOrgStatsUseCase;
        private readonly LeaveReviewUseCase _leaveReviewUseCase;
        private readonly IUserRepository _users;

        public ProfileController(
            ILogger<ProfileController> logger,
            ViewProfileUseCase viewProfileUseCase,
            ViewProfileDetailsUseCase viewProfileDetailsUseCase,
            UpdateProfileUseCase updateProfileUseCase,
            UpdateShelterProfileUseCase updateShelterProfileUseCase,
            ViewOrgStatsUseCase viewOrgStatsUseCase,
            LeaveReviewUseCase leaveReviewUseCase,
            IUserRepository users)
        {
            _logger = logger;
            _viewProfileUseCase = viewProfileUseCase;
            _viewProfileDetailsUseCase = viewProfileDetailsUseCase;
            _updateProfileUseCase = updateProfileUseCase;
            _updateShelterProfileUseCase = updateShelterProfileUseCase;
            _viewOrgStatsUseCase = viewOrgStatsUseCase;
            _leaveReviewUseCase = leaveReviewUseCase;
            _users = users;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(MyProfile));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyProfile(CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            if (authContext.UserId is null)
            {
                return Challenge();
            }

            return RedirectToAction(nameof(Details), new { id = authContext.UserId.Value });
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var profile = await _viewProfileDetailsUseCase.ExecuteAsync(new ViewProfileDetailsRequest(id), authContext, cancellationToken);

            if (profile == null)
            {
                return NotFound();
            }

            var model = new ProfileDetailsViewModel
            {
                UserId = profile.User.Id,
                DisplayName = profile.User.DisplayName,
                Email = profile.User.Email,
                Role = profile.User.Role,
                IsBlocked = profile.User.IsBlocked,
                Description = profile.ShelterProfile?.Description,
                Website = profile.ShelterProfile?.Website,
                Address = profile.ShelterProfile?.Address,
                Rating = profile.Rating,
                ReviewsCount = profile.ReviewsCount,
                CanEdit = authContext.UserId == profile.User.Id || authContext.Role == Role.Admin,
                Listings = profile.Listings
                    .Select(listing => new ListingWithStatusViewModel
                    {
                        Id = listing.Id,
                        Title = listing.Title,
                        AnimalType = listing.AnimalType,
                        Location = listing.Location,
                        ListedAt = listing.ListedAt,
                        IsUrgent = listing.IsUrgent,
                        PhotoUrl = listing.PrimaryPhotoUrl,
                        Status = listing.Status
                    })
                    .ToList()
            };

            if (profile.User.Role == Role.Shelter)
            {
                try
                {
                    ViewBag.Stats = await _viewOrgStatsUseCase.ExecuteAsync(new ViewOrgStatsRequest(id), authContext, cancellationToken);
                }
                catch
                {
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            if (authContext.UserId == null)
            {
                return Challenge();
            }

            var user = await _viewProfileUseCase.ExecuteAsync(new ViewProfileRequest(authContext.UserId.Value), authContext, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                DisplayName = user.DisplayName,
                IsShelter = user.Role == Role.Shelter
            };

            if (user.Role == Role.Shelter)
            {
                var profile = await _viewProfileDetailsUseCase.ExecuteAsync(
                    new ViewProfileDetailsRequest(authContext.UserId.Value),
                    authContext,
                    cancellationToken);

                model.Description = profile?.ShelterProfile?.Description;
                model.Website = profile?.ShelterProfile?.Website;
                model.Address = profile?.ShelterProfile?.Address;
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authContext = await GetAuthContextAsync(cancellationToken);
            if (authContext.UserId == null)
            {
                return Unauthorized();
            }

            try
            {
                if (authContext.Role == Role.Shelter)
                {
                    var request = new UpdateShelterProfileRequest(model.DisplayName, model.Description ?? string.Empty, model.Website, model.Address);
                    await _updateShelterProfileUseCase.ExecuteAsync(request, authContext, cancellationToken);
                }
                else
                {
                    var request = new UpdateProfileRequest(authContext.UserId.Value, model.DisplayName);
                    await _updateProfileUseCase.ExecuteAsync(request, authContext, cancellationToken);
                }

                _logger.LogInformation("Profile updated for user {UserId}", authContext.UserId);
                return RedirectToAction(nameof(Details), new { id = authContext.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for {UserId}", authContext.UserId);
                ModelState.AddModelError(string.Empty, "Помилка при збереженні даних.");
                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveReview(Guid listingId, byte rating, string comment, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            try
            {
                var request = new LeaveReviewRequest(listingId, rating, comment, true);
                await _leaveReviewUseCase.ExecuteAsync(request, authContext, cancellationToken);
                return RedirectToAction("Details", "Home", new { id = listingId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
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

            Enum.TryParse<Role>(roleString, out var role);

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
    }
}
