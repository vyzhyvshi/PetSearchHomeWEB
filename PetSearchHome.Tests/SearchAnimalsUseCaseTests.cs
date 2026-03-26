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
            _searchGatewayMock = new Mock<ISearchGateway>();

            _useCase = new SearchAnimalsUseCase(_searchGatewayMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenListingsExist_ReturnsListOfAnimals()
        {

            var filters = new SearchFilters();

            var mockDatabaseResult = new List<PetListing>
            {
                new PetListing { Id = Guid.NewGuid(), Title = "Rocky", AnimalType = "Dog" },
                new PetListing { Id = Guid.NewGuid(), Title = "Mira", AnimalType = "Cat" }
            };

            _searchGatewayMock
                .Setup(gateway => gateway.SearchAsync(filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDatabaseResult);

  
            var request = new SearchAnimalsRequest(filters);
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);



            Assert.NotNull(result); 
            Assert.Equal(2, result.Count); 
        }

        [Fact]
        public async Task ExecuteAsync_WhenNoAnimalsMatchFilters_ReturnsEmptyList()
        {

            var filters = new SearchFilters { AnimalType = "Parrot" };

            _searchGatewayMock
                .Setup(gateway => gateway.SearchAsync(filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing>());

            var request = new SearchAnimalsRequest(filters);
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result); 
        }
    }
}