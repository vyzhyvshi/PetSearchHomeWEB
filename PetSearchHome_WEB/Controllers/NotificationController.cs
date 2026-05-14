using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Notifications;
using PetSearchHome_WEB.Application.Shared;
using System.Security.Claims;

namespace PetSearchHome_WEB.Controllers
{
    [Authorize] 
    public class NotificationController : Controller
    {
        private readonly GetUserNotificationsUseCase _getNotificationsUseCase;

        public NotificationController(GetUserNotificationsUseCase getNotificationsUseCase)
        {
            _getNotificationsUseCase = getNotificationsUseCase;
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Auth"); 
            }

            var authContext = new AuthContext { UserId = userId };

            var result = await _getNotificationsUseCase.ExecuteAsync(new GetUserNotificationsRequest(), authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Value);
        }
    }
}