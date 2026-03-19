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
        // 1. Додаємо мок для токенів
        private readonly Mock<IAuthTokenService> _tokenServiceMock;
        private readonly LoginUseCase _useCase;

        public LoginUseCaseTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            // 2. Ініціалізуємо його
            _tokenServiceMock = new Mock<IAuthTokenService>();

            // 3. Передаємо третім параметром!
            _useCase = new LoginUseCase(
                _userRepoMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object);
        }

        // --- ПОЗИТИВНИЙ СЦЕНАРІЙ ---
        [Fact]
        public async Task ExecuteAsync_ValidCredentials_ReturnsResponse()
        {
            // 1. Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashed_pass", Role = Role.Person };

            var request = new LoginRequest(email, password);
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            // База знаходить користувача
            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);
            // Пароль підходить
            _passwordHasherMock.Setup(hasher => hasher.Verify(password, user.PasswordHash))
                               .Returns(true);

            // 2. Act
            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            // 3. Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
        }

        // --- НЕГАТИВНИЙ СЦЕНАРІЙ ---
        [Fact]
        public async Task ExecuteAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // 1. Arrange
            var email = "test@example.com";
            var password = "wrong_password";
            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashed_pass", Role = Role.Person };

            var request = new LoginRequest(email, password);
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            // Хеш НЕ співпадає (фейк повертає false)
            _passwordHasherMock.Setup(hasher => hasher.Verify(password, user.PasswordHash))
                               .Returns(false);

            // 2 & 3. Act & Assert (Очікуємо помилку доступу)
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _useCase.ExecuteAsync(request, authContext, CancellationToken.None));
        }
    }
}