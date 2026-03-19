using Moq;
using PetSearchHome_WEB.Application.Catalog; // Перевір, чи правильна папка
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class SearchAnimalsUseCaseTests
    {
        // Створюємо "фейковий" шлюз бази даних
        private readonly Mock<ISearchGateway> _searchGatewayMock;
        private readonly SearchAnimalsUseCase _useCase;

        public SearchAnimalsUseCaseTests()
        {
            _searchGatewayMock = new Mock<ISearchGateway>();

            // Передаємо фейковий шлюз у наш UseCase
            _useCase = new SearchAnimalsUseCase(_searchGatewayMock.Object);
        }

        // --- ПОЗИТИВНИЙ СЦЕНАРІЙ ---
        [Fact]
        public async Task ExecuteAsync_WhenListingsExist_ReturnsListOfAnimals()
        {
            // 1. Arrange (Підготовка)
            // Імітуємо фільтри, які користувач міг ввести на сайті
            var filters = new SearchFilters();

            // Імітуємо те, що могла б повернути реальна база даних
            var mockDatabaseResult = new List<PetListing>
            {
                new PetListing { Id = Guid.NewGuid(), Title = "Rocky", AnimalType = "Dog" },
                new PetListing { Id = Guid.NewGuid(), Title = "Mira", AnimalType = "Cat" }
            };

            // Вчимо наш фейк: коли UseCase попросить дані, віддай йому mockDatabaseResult
            _searchGatewayMock
                .Setup(gateway => gateway.SearchAsync(filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDatabaseResult);

            // 2. Act (Дія)
            // 2. Act (Дія)
            // Загортаємо фільтри у Request і створюємо контекст гостя
            var request = new SearchAnimalsRequest(filters);
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);



            // 3. Assert (Перевірка результату)
            Assert.NotNull(result); // Результат не має бути пустим (null)
            Assert.Equal(2, result.Count); // Ми очікуємо рівно 2 картки тварин
            // Примітка: якщо твій UseCase повертає не список, а об'єкт із властивістю (наприклад, result.Results.Count), 
            // просто додай .Results сюди.
        }

        // --- НЕГАТИВНИЙ/ГРАНИЧНИЙ СЦЕНАРІЙ ---
        [Fact]
        public async Task ExecuteAsync_WhenNoAnimalsMatchFilters_ReturnsEmptyList()
        {
            // 1. Arrange (Підготовка)
            // Шукаємо папугу, якого в базі немає
            var filters = new SearchFilters { AnimalType = "Parrot" };

            // Вчимо фейк повертати порожній список
            _searchGatewayMock
                .Setup(gateway => gateway.SearchAsync(filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing>());

            // 2. Act (Дія)
            var request = new SearchAnimalsRequest(filters);
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            // 3. Assert (Перевірка результату)
            Assert.NotNull(result);
            Assert.Empty(result); // Перевіряємо, що список дійсно порожній, і програма не впала з помилкою
        }
    }
}