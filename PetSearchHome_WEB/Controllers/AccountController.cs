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

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new LoginRequest(model.Email, model.Password);

                var response = await _loginUseCase.ExecuteAsync(request, GetGuestContext(), cancellationToken);

                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Role, Role.Person.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User {Email} logged in successfully.", model.Email);

                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Failed login attempt for {Email}.", model.Email);
                ModelState.AddModelError(string.Empty, "Неправильний email або пароль.");
                return View(model);
            }
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

            try
            {
                var request = new RegisterUserRequest(model.Email, model.DisplayName, model.Password);
                await _registerUserUseCase.ExecuteAsync(request, GetGuestContext(), cancellationToken);

                _logger.LogInformation("New individual account created for {Email}.", model.Email);

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for individual {Email}.", model.Email);
                ModelState.AddModelError(string.Empty, "Сталася помилка під час реєстрації. Можливо, email вже використовується.");
                return View(model);
            }
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

            try
            {
                var request = new RegisterShelterRequest(model.Email, model.ShelterName, model.Password);
                await _registerShelterUseCase.ExecuteAsync(request, GetGuestContext(), cancellationToken);

                _logger.LogInformation("New shelter account created for {Email}.", model.Email);
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for shelter {Email}.", model.Email);
                ModelState.AddModelError(string.Empty, "Сталася помилка під час реєстрації.");
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var authContext = GetAuthContext();

            if (authContext.UserId.HasValue)
            {
                try
                {
                    var request = new LogoutRequest(authContext.UserId.Value);
                    await _logoutUseCase.ExecuteAsync(request, authContext, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error occurred during UseCase logout for user {UserId}", authContext.UserId);
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User {UserId} logged out successfully.", authContext.UserId);

            // Повертаємо на головну сторінку
            return RedirectToAction("Index", "Home");
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