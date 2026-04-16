using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Models.Admin;
using PetSearchHome_WEB.Security;

namespace PetSearchHome_WEB.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
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

            var pendingResult = await _getPendingListingsUseCase.ExecuteAsync(new GetPendingListingsRequest(), authContext, cancellationToken);
            var complaintsResult = await _getOpenComplaintsUseCase.ExecuteAsync(new GetOpenComplaintsRequest(), authContext, cancellationToken);

            if (!pendingResult.IsSuccess || !complaintsResult.IsSuccess)
            {
                return Forbid();
            }

            var viewModel = new AdminDashboardViewModel
            {
                PendingListings = pendingResult.Value!,
                OpenComplaints = complaintsResult.Value!
            };

            return View(viewModel);
        }

        // POST: /Admin/ModerateListing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModerateListing(Guid listingId, bool approve, string? reason, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            var request = new ModerateListingRequest(listingId, approve, reason);

            var result = await _moderateListingUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed moderation attempt: {Error}", result.ErrorMessage);
                if (result.ErrorMessage!.Contains("не знайдено")) return NotFound();
                return Forbid();
            }

            SetSuccessMessage($"Оголошення успішно {(approve ? "схвалено" : "відхилено")}.");
            return RedirectToAction(nameof(Dashboard));
        }

        // POST: /Admin/BlockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(Guid targetUserId, bool block, CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            var request = new BlockUserRequest(targetUserId, block);

            var result = await _blockUserUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed block user attempt: {Error}", result.ErrorMessage);
                return Forbid();
            }

            SetSuccessMessage($"Користувача {(block ? "заблоковано" : "розблоковано")}.");
            return RedirectToAction(nameof(Dashboard));
        }

        // POST: /Admin/HandleComplaint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleComplaint(Guid complaintId, string resolution, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(resolution))
            {
                SetErrorMessage("Вкажіть резолюцію (рішення) щодо скарги.");
                return RedirectToAction(nameof(Dashboard));
            }

            var authContext = GetAuthContext();
            var request = new HandleComplaintRequest(complaintId, resolution);

            var result = await _handleComplaintUseCase.ExecuteAsync(request, authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed complaint handling attempt: {Error}", result.ErrorMessage);
                return Forbid();
            }

            SetSuccessMessage("Скаргу успішно оброблено.");
            return RedirectToAction(nameof(Dashboard));
        }
    }
}