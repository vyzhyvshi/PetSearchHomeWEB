using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using System.Security.Claims;

namespace PetSearchHome_WEB.Controllers
{
    public abstract class AppController : Controller
    {
        protected const string SuccessMessageKey = "SuccessMessage";
        protected const string ErrorMessageKey = "ErrorMessage";

        protected AuthContext GetGuestContext()
        {
            return new AuthContext { UserId = null, Role = Role.Guest };
        }

        protected AuthContext GetAuthContext()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return GetGuestContext();
            }

            return new AuthContext
            {
                UserId = TryGetUserId(),
                Role = TryGetRole()
            };
        }

        protected async Task<AuthContext> GetAuthContextAsync(CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();
            if (authContext.UserId is null)
            {
                return authContext;
            }

            var users = HttpContext.RequestServices.GetService(typeof(IUserRepository)) as IUserRepository;
            if (users is null)
            {
                return authContext;
            }

            var existing = await users.GetByIdAsync(authContext.UserId.Value, cancellationToken);
            if (existing is not null)
            {
                return new AuthContext { UserId = existing.Id, Role = existing.Role };
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await users.GetByEmailAsync(email, cancellationToken);
                if (user is not null)
                {
                    return new AuthContext { UserId = user.Id, Role = user.Role };
                }
            }

            return authContext;
        }

        protected void SetSuccessMessage(string message)
        {
            TempData[SuccessMessageKey] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData[ErrorMessageKey] = message;
        }

        private Guid? TryGetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) && userId != Guid.Empty
                ? userId
                : null;
        }

        private Role TryGetRole()
        {
            var roleString = User.FindFirstValue(ClaimTypes.Role);
            return !string.IsNullOrWhiteSpace(roleString) && Enum.TryParse<Role>(roleString, true, out var role)
                ? role
                : Role.Person;
        }
    }
}
