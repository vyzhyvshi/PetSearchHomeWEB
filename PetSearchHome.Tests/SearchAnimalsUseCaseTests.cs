using Moq;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class SearchAnimalsUseCaseTests
    {
        private readonly Mock<ISearchGateway> _searchGatewayMock;
        private readonly SearchAnimalsUseCase _useCase;

        public SearchAnimalsUseCaseTests()
        {
            _searchGatewayMock = new();
            _useCase = new SearchAnimalsUseCase(_searchGatewayMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenListingsExist_ReturnsListOfAnimals()
        {
            SearchFilters filters = new();

            // Короткий синтаксис для виправлення IDE0028 та IDE0090
            List<PetListing> mockDatabaseResult = new()
            {
                new() { Id = Guid.NewGuid(), Title = "Rocky", AnimalType = "Dog" },
                new() { Id = Guid.NewGuid(), Title = "Mira", AnimalType = "Cat" }
            };

            _searchGatewayMock
                .Setup(gateway => gateway.SearchAsync(filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDatabaseResult);

            SearchAnimalsRequest request = new(filters);
            AuthContext authContext = new() { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            // ВИПРАВЛЕННЯ: Тепер ми перевіряємо об'єкт Result, а список лежить у result.Value
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task ExecuteAsync_WhenNoAnimalsMatchFilters_ReturnsEmptyList()
        {
            SearchFilters filters = new() { AnimalType = "Parrot" };

            _searchGatewayMock
                .Setup(gateway => gateway.SearchAsync(filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            SearchAnimalsRequest request = new(filters);
            AuthContext authContext = new() { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }
    }
}