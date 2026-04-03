using Moq;
using PetSearchHome_WEB.Application.Profiles;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class ViewProfileUseCaseTests
    {
        private readonly Mock<IUserRepository> _usersMock;
        private readonly ViewProfileUseCase _useCase;

        public ViewProfileUseCaseTests()
        {
            _usersMock = new Mock<IUserRepository>();
            _useCase = new ViewProfileUseCase(_usersMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserNotFound_ReturnsFailure()
        {
            _usersMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var request = new ViewProfileRequest(Guid.NewGuid());
            var authContext = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.False(result.IsSuccess);
            Assert.Contains("не знайдено", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserFound_ReturnsSuccessWithUser()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, DisplayName = "Іван", Email = "ivan@test.com", Role = Role.Person };

            _usersMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var request = new ViewProfileRequest(userId);
            var authContext = new AuthContext { UserId = userId, Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal("Іван", result.Value.DisplayName);
            Assert.Equal(Role.Person, result.Value.Role);
        }
    }
}