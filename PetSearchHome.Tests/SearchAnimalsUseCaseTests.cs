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

            // Короткий синтаксис для виправлення IDE0028
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

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task ExecuteAsync_WhenNoAnimalsMatchFilters_ReturnsEmptyList()
        {
            SearchFilters filters = new() { AnimalType = "Parrot" };

            _searchGatewayMock
                .Setup(gateway => gateway.SearchAsync(filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing>());

            SearchAnimalsRequest request = new(filters);
            AuthContext authContext = new() { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}