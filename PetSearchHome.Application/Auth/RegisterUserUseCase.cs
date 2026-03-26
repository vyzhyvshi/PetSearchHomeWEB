using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Auth
{
    public sealed record RegisterUserRequest(string Email, string DisplayName, string Password);

    public class RegisterUserUseCase : IUseCase<RegisterUserRequest, Guid>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public RegisterUserUseCase(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<Guid> ExecuteAsync(RegisterUserRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {

            if (authContext.Role == Role.Guest || authContext.Role == Role.Admin || authContext.Role == Role.Person || authContext.Role == Role.Shelter)
            {
                var existingUser = await _users.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null)
                {
                    Exception exception = new InvalidOperationException("Користувач з таким email вже існує.");
                    throw exception;
                }
                var user = new User
                {
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    PasswordHash = _hasher.Hash(request.Password),
                    Role = Role.Person
                };

                await _users.AddAsync(user, cancellationToken);
                var persisted = await _users.GetByEmailAsync(request.Email, cancellationToken);
                return persisted?.Id ?? user.Id;
            }

            throw new UnauthorizedAccessException("Cannot register user for this role.");
        }
    }
}
