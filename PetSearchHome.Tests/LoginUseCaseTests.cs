using Moq;
using Xunit;
using PetSearchHome_WEB.Application.Auth;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome.Tests
{
    public class LoginUseCaseTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IAuthTokenService> _tokenServiceMock;
        private readonly LoginUseCase _useCase;

        public LoginUseCaseTests()
        {
            _userRepoMock = new();
            _passwordHasherMock = new();
            _tokenServiceMock = new();

            _useCase = new LoginUseCase(
                _userRepoMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ValidCredentials_ReturnsSuccess()
        {
            var email = "test@example.com";
            var password = "password123";
            User user = new() { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashed_pass", Role = Role.Person };

            LoginRequest request = new(email, password);
            AuthContext authContext = new() { UserId = null, Role = Role.Guest };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);
            _passwordHasherMock.Setup(hasher => hasher.Verify(password, user.PasswordHash))
                               .Returns(true);

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(user.Id, result.Value.UserId);
        }

        [Fact]
        public async Task ExecuteAsync_InvalidPassword_ReturnsFailure()
        {
            var email = "test@example.com";
            var password = "wrong_password";
            User user = new() { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashed_pass", Role = Role.Person };

            LoginRequest request = new(email, password);
            AuthContext authContext = new() { UserId = null, Role = Role.Guest };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            _passwordHasherMock.Setup(hasher => hasher.Verify(password, user.PasswordHash))
                               .Returns(false);

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("Неправильний email або пароль.", result.ErrorMessage);
        }
    }
}