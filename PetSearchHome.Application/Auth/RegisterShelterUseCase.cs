using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record RegisterShelterRequest(string Email, string DisplayName, string Password);

    public class RegisterShelterUseCase : IUseCase<RegisterShelterRequest, Result<Guid>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public RegisterShelterUseCase(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<Result<Guid>> ExecuteAsync(RegisterShelterRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.Role == Role.Guest || authContext.Role == Role.Admin)
            {
                var existingUser = await _users.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null)
                {
                    return Result.Failure<Guid>("Користувач з таким email вже існує.");
                }

                User user = new()
                {
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    PasswordHash = _hasher.Hash(request.Password),
                    Role = Role.Shelter
                };

                await _users.AddAsync(user, cancellationToken);
                var persisted = await _users.GetByEmailAsync(request.Email, cancellationToken);

                return persisted?.Id ?? user.Id;
            }

            return Result.Failure<Guid>("У вас немає прав для реєстрації притулку.");
        }
    }
}
