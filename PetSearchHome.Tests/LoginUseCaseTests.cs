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
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenServiceMock = new Mock<IAuthTokenService>();

            _useCase = new LoginUseCase(
                _userRepoMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ValidCredentials_ReturnsResponse()
        {
            var email = "test@example.com";
            var password = "password123";
            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashed_pass", Role = Role.Person };

            var request = new LoginRequest(email, password);
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);
            _passwordHasherMock.Setup(hasher => hasher.Verify(password, user.PasswordHash))
                               .Returns(true);

   
            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
        }


        [Fact]
        public async Task ExecuteAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {

            var email = "test@example.com";
            var password = "wrong_password";
            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashed_pass", Role = Role.Person };

            var request = new LoginRequest(email, password);
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            _passwordHasherMock.Setup(hasher => hasher.Verify(password, user.PasswordHash))
                               .Returns(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _useCase.ExecuteAsync(request, authContext, CancellationToken.None));
        }
    }
}