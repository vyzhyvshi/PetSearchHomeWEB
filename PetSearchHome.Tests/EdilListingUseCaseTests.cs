using Moq;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class EditListingUseCaseTests
    {
        private readonly Mock<IListingRepository> _listingsMock;
        private readonly EditListingUseCase _useCase;

        public EditListingUseCaseTests()
        {
            _listingsMock = new Mock<IListingRepository>();
            _useCase = new EditListingUseCase(_listingsMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenListingNotFound_ReturnsFailure()
        {

            _listingsMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PetListing?)null);

            
            var request = new EditListingRequest(Guid.NewGuid(), "Нова назва", "Dog", "Київ", "Опис", false, Array.Empty<string>());
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
            var listingId = Guid.NewGuid();
            var listing = new PetListing { Id = listingId, OwnerId = ownerId };

            _listingsMock.Setup(repo => repo.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(listing);

            
            var request = new EditListingRequest(listingId, "Нова назва", "Dog", "Київ", "Опис", false, Array.Empty<string>());
            var authContext = new AuthContext { UserId = hackerId, Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.False(result.IsSuccess);
            Assert.Contains("немає прав", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            _listingsMock.Verify(repo => repo.UpdateAsync(It.IsAny<PetListing>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenValidRequest_UpdatesListingAndSetsToModeration()
        {
            var ownerId = Guid.NewGuid();
            var listingId = Guid.NewGuid();

            var listing = new PetListing { Id = listingId, OwnerId = ownerId, Status = ListingStatus.Published };

            _listingsMock.Setup(repo => repo.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(listing);

            var request = new EditListingRequest(listingId, "Новий песик", "Dog", "Львів", "Новий опис", true, Array.Empty<string>());
            var authContext = new AuthContext { UserId = ownerId, Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);

            _listingsMock.Verify(repo => repo.UpdateAsync(
                It.Is<PetListing>(l => l.Title == "Новий песик" && l.Status == ListingStatus.PendingModeration),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}