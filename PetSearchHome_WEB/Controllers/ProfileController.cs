using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Profiles;
using PetSearchHome_WEB.Application.Reviews;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Models.Profile;
using System.Security.Claims;

namespace PetSearchHome_WEB.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly ViewProfileUseCase _viewProfileUseCase;
        private readonly UpdateProfileUseCase _updateProfileUseCase;
        private readonly UpdateShelterProfileUseCase _updateShelterProfileUseCase;
        private readonly ViewOrgStatsUseCase _viewOrgStatsUseCase;
        private readonly LeaveReviewUseCase _leaveReviewUseCase;

        public ProfileController(
            ILogger<ProfileController> logger,
            ViewProfileUseCase viewProfileUseCase,
            UpdateProfileUseCase updateProfileUseCase,
            UpdateShelterProfileUseCase updateShelterProfileUseCase,
            ViewOrgStatsUseCase viewOrgStatsUseCase,
            LeaveReviewUseCase leaveReviewUseCase)
        {
            _logger = logger;
            _viewProfileUseCase = viewProfileUseCase;
            _updateProfileUseCase = updateProfileUseCase;
            _updateShelterProfileUseCase = updateShelterProfileUseCase;
            _viewOrgStatsUseCase = viewOrgStatsUseCase;
            _leaveReviewUseCase = leaveReviewUseCase;
        }

        // GET: /Profile/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            var user = await _viewProfileUseCase.ExecuteAsync(new ViewProfileRequest(id), authContext, cancellationToken);

            if (user == null) return NotFound();

            if (user.Role == Role.Shelter)
            {
                try
                {
                    ViewBag.Stats = await _viewOrgStatsUseCase.ExecuteAsync(new ViewOrgStatsRequest(id), authContext, cancellationToken);
                }
                catch {}
            }

            return View(user);
        }

        // GET: /Profile/Edit
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var authContext = GetAuthContext();
            if (authContext.UserId == null) return Challenge();

            var user = await _viewProfileUseCase.ExecuteAsync(new ViewProfileRequest(authContext.UserId.Value), authContext);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                DisplayName = user.DisplayName,
                IsShelter = user.Role == Role.Shelter
            };

            return View(model);
        }

        // POST: /Profile/Edit
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return View(model);

            var authContext = GetAuthContext();
            if (authContext.UserId == null) return Unauthorized();

            try
            {
                if (authContext.Role == Role.Shelter)
                {
                    var request = new UpdateShelterProfileRequest(model.DisplayName, model.Description ?? "", model.Website, model.Address);
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
                ModelState.AddModelError("", "Помилка при збереженні даних.");
                return View(model);
            }
        }

        // POST: /Profile/LeaveReview
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveReview(Guid listingId, byte rating, string comment, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
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

        private AuthContext GetAuthContext()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdString, out Guid userId);

            var roleString = User.FindFirstValue(ClaimTypes.Role);
            Enum.TryParse<Role>(roleString, out var role);

            return new AuthContext
            {
                UserId = userId == Guid.Empty ? null : userId,
                Role = User.Identity?.IsAuthenticated == true ? role : Role.Guest
            };
        }
    }
}