using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Auth;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Models.Auth;
using System.Security.Claims;

namespace PetSearchHome_WEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly LoginUseCase _loginUseCase;
        private readonly RegisterUserUseCase _registerUserUseCase;
        private readonly RegisterShelterUseCase _registerShelterUseCase;
        private readonly LogoutUseCase _logoutUseCase;

        public AccountController(
            ILogger<AccountController> logger,
            LoginUseCase loginUseCase,
            RegisterUserUseCase registerUserUseCase,
            RegisterShelterUseCase registerShelterUseCase,
            LogoutUseCase logoutUseCase)
        {
            _logger = logger;
            _loginUseCase = loginUseCase;
            _registerUserUseCase = registerUserUseCase;
            _registerShelterUseCase = registerShelterUseCase;
            _logoutUseCase = logoutUseCase;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // GET: /Account/GuestEntry
        [HttpGet]
        public IActionResult GuestEntry()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            LoginRequest request = new(model.Email, model.Password);

            var result = await _loginUseCase.ExecuteAsync(request, GetGuestContext(), cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed login attempt for {Email}: {Error}", model.Email, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Неправильний email або пароль.");
                return View(model);
            }

            var response = result.Value!;

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, response.Role.ToString())
            };

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties authProperties = new() { IsPersistent = true };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("User {Email} logged in successfully.", model.Email);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/RegisterIndividual
        [HttpGet]
        public IActionResult RegisterIndividual()
        {
            return View();
        }

        // POST: /Account/RegisterIndividual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterIndividual(RegisterIndividualViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            RegisterUserRequest request = new(model.Email, model.DisplayName, model.Password);
            var result = await _registerUserUseCase.ExecuteAsync(request, GetGuestContext(), cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Registration failed for individual {Email}: {Error}", model.Email, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Сталася помилка під час реєстрації.");
                return View(model);
            }

            _logger.LogInformation("New individual account created for {Email}.", model.Email);

            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/RegisterShelter
        [HttpGet]
        public IActionResult RegisterShelter()
        {
            return View();
        }

        // POST: /Account/RegisterShelter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterShelter(RegisterShelterViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            RegisterShelterRequest request = new(model.Email, model.ShelterName, model.Password);
            var result = await _registerShelterUseCase.ExecuteAsync(request, GetGuestContext(), cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Registration failed for shelter {Email}: {Error}", model.Email, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Сталася помилка.");
                return View(model);
            }

            _logger.LogInformation("New shelter account created for {Email}.", model.Email);
            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            if (authContext.UserId.HasValue)
            {
                LogoutRequest request = new(authContext.UserId.Value);
                var result = await _logoutUseCase.ExecuteAsync(request, authContext, cancellationToken);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Error occurred during UseCase logout for user {UserId}: {Error}", authContext.UserId, result.ErrorMessage);
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User {UserId} logged out successfully.", authContext.UserId);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout 
        [HttpGet]
        public IActionResult Logout()
        {
            return View();
        }

        private AuthContext GetGuestContext()
        {
            return new AuthContext { UserId = null, Role = Role.Guest };
        }

        private AuthContext GetAuthContext()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                return GetGuestContext();
            }

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