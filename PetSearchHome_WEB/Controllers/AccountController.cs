using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PetSearchHome_WEB.Application.Auth;
using PetSearchHome_WEB.Application.Profiles;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Filters;
using PetSearchHome_WEB.Models.Auth;
using PetSearchHome_WEB.Security;
using System.Security.Claims;

namespace PetSearchHome_WEB.Controllers
{
    public class AccountController : AppController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly LoginUseCase _loginUseCase;
        private readonly RegisterUserUseCase _registerUserUseCase;
        private readonly RegisterShelterUseCase _registerShelterUseCase;
        private readonly UpdateShelterProfileUseCase _updateShelterProfileUseCase;
        private readonly LogoutUseCase _logoutUseCase;
        private readonly RequestPasswordResetUseCase _requestPasswordResetUseCase;
        private readonly ResetPasswordUseCase _resetPasswordUseCase;
        private readonly IUserRepository _users;

        public AccountController(
            ILogger<AccountController> logger,
            LoginUseCase loginUseCase,
            RegisterUserUseCase registerUserUseCase,
            RegisterShelterUseCase registerShelterUseCase,
            UpdateShelterProfileUseCase updateShelterProfileUseCase,
            LogoutUseCase logoutUseCase,
            RequestPasswordResetUseCase requestPasswordResetUseCase,
            ResetPasswordUseCase resetPasswordUseCase,
            IUserRepository users)
        {
            _logger = logger;
            _loginUseCase = loginUseCase;
            _registerUserUseCase = registerUserUseCase;
            _registerShelterUseCase = registerShelterUseCase;
            _updateShelterProfileUseCase = updateShelterProfileUseCase;
            _logoutUseCase = logoutUseCase;
            _requestPasswordResetUseCase = requestPasswordResetUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
            _users = users;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // GET: /Account/GuestEntry
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GuestEntry()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [RateLimiting(5)]
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
        [AllowAnonymous]
        public IActionResult RegisterIndividual()
        {
            return View();
        }

        // POST: /Account/RegisterIndividual
        [HttpPost]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public IActionResult RegisterShelter()
        {
            return View();
        }

        // POST: /Account/RegisterShelter
        [HttpPost]
        [AllowAnonymous]
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
            var createdContext = await BuildShelterContextAsync(model.Email, cancellationToken);
            if (createdContext is PetSearchHome_WEB.Application.Shared.AuthContext shelterContext)
            {
                await _updateShelterProfileUseCase.ExecuteAsync(
                    new UpdateShelterProfileRequest(model.ShelterName, model.Description ?? string.Empty, model.Website, model.Address),
                    shelterContext,
                    cancellationToken);
            }
            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize(Roles = RoleNames.AuthenticatedUser)]
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
        [Authorize(Roles = RoleNames.AuthenticatedUser)]
        public IActionResult Logout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _requestPasswordResetUseCase.ExecuteAsync(new RequestPasswordResetRequest(model.Email), GetGuestContext(), cancellationToken);
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не вдалося створити посилання для скидання пароля.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Посилання для скидання пароля створено. У dev-режимі воно також записане в лог.";
            return RedirectToAction(nameof(ResetPassword), new { email = model.Email, token = result.Value });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _resetPasswordUseCase.ExecuteAsync(
                new ResetPasswordRequest(model.Email, model.Token, model.NewPassword),
                GetGuestContext(),
                cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не вдалося змінити пароль.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Пароль змінено. Увійдіть з новим паролем.";
            return RedirectToAction(nameof(Login));
        }

        private async Task<PetSearchHome_WEB.Application.Shared.AuthContext?> BuildShelterContextAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _users.GetByEmailAsync(email, cancellationToken);
            if (user is null)
            {
                return null;
            }

            return new PetSearchHome_WEB.Application.Shared.AuthContext
            {
                UserId = user.Id,
                Role = Role.Shelter
            };
        }

    }
}
