using Moq;
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
            // Якщо твій LogoutUseCase потребує параметрів (наприклад, IAuthTokenService), 
            // студія підкреслить це червоним. Тоді скажеш мені!
            _useCase = new LogoutUseCase();
        }

        [Fact]
        public async Task ExecuteAsync_ValidRequest_ExecutesWithoutErrors()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new LogoutRequest(userId);
            var authContext = new AuthContext { UserId = userId, Role = Role.Person };

            // Act
            var exception = await Record.ExceptionAsync(() =>
                _useCase.ExecuteAsync(request, authContext, CancellationToken.None));

            // Assert
            Assert.Null(exception); // Перевіряємо, що під час виходу не виникло жодних збоїв
        }
    }
}