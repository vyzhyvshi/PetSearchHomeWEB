using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Models.Admin;

namespace PetSearchHome_WEB.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : AppController
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ModerateListingUseCase _moderateListingUseCase;
        private readonly BlockUserUseCase _blockUserUseCase;
        private readonly HandleComplaintUseCase _handleComplaintUseCase;
        private readonly GetPendingListingsUseCase _getPendingListingsUseCase;
        private readonly GetOpenComplaintsUseCase _getOpenComplaintsUseCase;

        public AdminController(
            ILogger<AdminController> logger,
            ModerateListingUseCase moderateListingUseCase,
            BlockUserUseCase blockUserUseCase,
            HandleComplaintUseCase handleComplaintUseCase,
            GetPendingListingsUseCase getPendingListingsUseCase, 
            GetOpenComplaintsUseCase getOpenComplaintsUseCase)   
        {
            _logger = logger;
            _moderateListingUseCase = moderateListingUseCase;
            _blockUserUseCase = blockUserUseCase;
            _handleComplaintUseCase = handleComplaintUseCase;
            _getPendingListingsUseCase = getPendingListingsUseCase;
            _getOpenComplaintsUseCase = getOpenComplaintsUseCase;
        }

        // GET: /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            _logger.LogInformation("Admin dashboard accessed by user {UserId}", authContext.UserId);

            try
            {
                var pendingListings = await _getPendingListingsUseCase.ExecuteAsync(new GetPendingListingsRequest(), authContext, cancellationToken);
                var openComplaints = await _getOpenComplaintsUseCase.ExecuteAsync(new GetOpenComplaintsRequest(), authContext, cancellationToken);

                var viewModel = new AdminDashboardViewModel
                {
                    PendingListings = pendingListings,
                    OpenComplaints = openComplaints
                };

                return View(viewModel);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized dashboard access attempt by user {UserId}.", authContext.UserId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading admin dashboard for user {UserId}", authContext.UserId);
                return View("Error");
            }
        }

        // POST: /Admin/ModerateListing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModerateListing(Guid listingId, bool approve, string? reason, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            try
            {
                var request = new ModerateListingRequest(listingId, approve, reason);
                await _moderateListingUseCase.ExecuteAsync(request, authContext, cancellationToken);

                _logger.LogInformation("Listing {ListingId} moderated. Approved: {Approve}. Admin: {AdminId}",
                    listingId, approve, authContext.UserId);

                return RedirectToAction(nameof(Dashboard));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Listing {ListingId} not found during moderation.", listingId);
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized moderation attempt by user {UserId}.", authContext.UserId);
                return Forbid();
            }
        }

        // POST: /Admin/BlockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(Guid targetUserId, bool block, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            try
            {
                var request = new BlockUserRequest(targetUserId, block);
                await _blockUserUseCase.ExecuteAsync(request, authContext, cancellationToken);

                _logger.LogInformation("User {TargetUserId} block status set to {Block} by admin {AdminId}.",
                    targetUserId, block, authContext.UserId);

                return RedirectToAction(nameof(Dashboard));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized block user attempt by user {UserId}.", authContext.UserId);
                return Forbid();
            }
        }

        // POST: /Admin/HandleComplaint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleComplaint(Guid complaintId, string resolution, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(resolution))
            {
                ModelState.AddModelError(string.Empty, "Вкажіть резолюцію (рішення) щодо скарги.");
                return RedirectToAction(nameof(Dashboard));
            }

            var authContext = GetAuthContext();

            try
            {
                var request = new HandleComplaintRequest(complaintId, resolution);
                await _handleComplaintUseCase.ExecuteAsync(request, authContext, cancellationToken);

                _logger.LogInformation("Complaint {ComplaintId} resolved by admin {AdminId}.", complaintId, authContext.UserId);

                return RedirectToAction(nameof(Dashboard));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized complaint handling attempt by user {UserId}.", authContext.UserId);
                return Forbid();
            }
        }

    }
}
