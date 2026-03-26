using Xunit;
using PetSearchHome_WEB.Application.Auth;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome.Tests
{
    public class LogoutUseCaseTests
    {
        private readonly LogoutUseCase _useCase;

        public LogoutUseCaseTests()
        {
            _useCase = new LogoutUseCase();
        }

        [Fact]
        public async Task ExecuteAsync_ValidRequest_ReturnsSuccess()
        {
            var userId = Guid.NewGuid();
            LogoutRequest request = new(userId);
            AuthContext authContext = new() { UserId = userId, Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }
    }
}