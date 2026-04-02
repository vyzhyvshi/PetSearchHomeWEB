using Moq;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class DeleteListingUseCaseTests
    {
        private readonly Mock<IListingRepository> _listingsMock;
        private readonly DeleteListingUseCase _useCase;

        public DeleteListingUseCaseTests()
        {
            _listingsMock = new Mock<IListingRepository>();
            _useCase = new DeleteListingUseCase(_listingsMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenListingNotFound_ReturnsFailure()
        {
            _listingsMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PetListing?)null);

            var request = new DeleteListingRequest(Guid.NewGuid());
            var authContext = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.False(result.IsSuccess);
            Assert.Contains("не знайдено", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsNotOwner_ReturnsFailure()
        {
            var ownerId = Guid.NewGuid();
            var hackerId = Guid.NewGuid();
            var listing = new PetListing { Id = Guid.NewGuid(), OwnerId = ownerId };

            _listingsMock.Setup(repo => repo.GetByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(listing);

            var request = new DeleteListingRequest(listing.Id);
            var authContext = new AuthContext { UserId = hackerId, Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.False(result.IsSuccess);
            Assert.Contains("немає прав", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            _listingsMock.Verify(repo => repo.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}