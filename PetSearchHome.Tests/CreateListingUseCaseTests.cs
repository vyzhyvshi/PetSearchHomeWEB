using Moq;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class CreateListingUseCaseTests
    {
        private readonly Mock<IListingRepository> _listingsMock;
        private readonly Mock<IModerationQueue> _queueMock;
        private readonly CreateListingUseCase _useCase;

        public CreateListingUseCaseTests()
        {
            _listingsMock = new Mock<IListingRepository>();
            _queueMock = new Mock<IModerationQueue>();
            _useCase = new CreateListingUseCase(_listingsMock.Object, _queueMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserNotAuthorized_ReturnsFailure()
        {
            
            var request = new CreateListingRequest("Рекс", "Dog", "Київ", "Хороший хлопчик", false, Array.Empty<string>());
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.False(result.IsSuccess);
            Assert.Contains("авторизація", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExecuteAsync_WhenValidRequest_AddsListingAndReturnsSuccess()
        {
            
            var request = new CreateListingRequest("Мурка", "Cat", "Львів", "Дуже лагідна", true, Array.Empty<string>());
            var authContext = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            _listingsMock.Verify(repo => repo.AddAsync(It.IsAny<PetListing>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}