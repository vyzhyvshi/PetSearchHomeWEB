using Moq;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class AdminModerationUseCaseTests
    {
        [Fact]
        public async Task GetPendingListings_WhenAdmin_ReturnsList()
        {
            var mockRepo = new Mock<IListingRepository>();
            mockRepo.Setup(r => r.ListByStatusAsync(ListingStatus.PendingModeration, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing> { new PetListing(), new PetListing() });

            var useCase = new GetPendingListingsUseCase(mockRepo.Object);
            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Admin };

            var result = await useCase.ExecuteAsync(new GetPendingListingsRequest(), auth);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.Count);
        }

        [Fact]
        public async Task GetPendingListings_WhenGuest_ReturnsFailure()
        {
            var useCase = new GetPendingListingsUseCase(new Mock<IListingRepository>().Object);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await useCase.ExecuteAsync(new GetPendingListingsRequest(), auth);

            Assert.False(result.IsSuccess); 
        }

        [Fact]
        public async Task ModerateListing_WhenAdmin_ApprovesSuccessfully()
        {
            var mockListings = new Mock<IListingRepository>();
            mockListings.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PetListing { Id = Guid.NewGuid(), Status = ListingStatus.PendingModeration });

            var useCase = new ModerateListingUseCase(mockListings.Object, new Mock<INotificationGateway>().Object, new Mock<IAuditLogGateway>().Object);
            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Admin };

            var result = await useCase.ExecuteAsync(new ModerateListingRequest(Guid.NewGuid(), true, "Ок"), auth);

            Assert.True(result.IsSuccess);
            mockListings.Verify(r => r.UpdateAsync(It.Is<PetListing>(l => l.Status == ListingStatus.Published), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ModerateListing_WhenPerson_ReturnsFailure()
        {
            var useCase = new ModerateListingUseCase(new Mock<IListingRepository>().Object, new Mock<INotificationGateway>().Object, new Mock<IAuditLogGateway>().Object);
            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };

            var result = await useCase.ExecuteAsync(new ModerateListingRequest(Guid.NewGuid(), true, null), auth);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task HandleComplaint_WhenAdmin_ResolvesSuccessfully()
        {
            var mockComplaints = new Mock<IComplaintRepository>();
            var useCase = new HandleComplaintUseCase(mockComplaints.Object, new Mock<IAuditLogGateway>().Object);
            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Admin };

            var result = await useCase.ExecuteAsync(new HandleComplaintRequest(Guid.NewGuid(), "Видалено"), auth);

            Assert.True(result.IsSuccess);
            mockComplaints.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), "Видалено", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleComplaint_WhenGuest_ReturnsFailure()
        {
            var useCase = new HandleComplaintUseCase(new Mock<IComplaintRepository>().Object, new Mock<IAuditLogGateway>().Object);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await useCase.ExecuteAsync(new HandleComplaintRequest(Guid.NewGuid(), "Видалено"), auth);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task SearchUsers_WhenAdmin_ReturnsUsers()
        {
            var mockUsers = new Mock<IUserRepository>();
            mockUsers.Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { new User() });

            var mockListings = new Mock<IListingRepository>();
            mockListings.Setup(r => r.ListByOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing> { new PetListing() });

            var useCase = new SearchUsersWithListingsUseCase(mockUsers.Object, mockListings.Object);
            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Admin };

            var result = await useCase.ExecuteAsync(new SearchUsersWithListingsRequest("Іван"), auth);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SearchUsers_WhenPerson_ReturnsFailure()
        {
            var useCase = new SearchUsersWithListingsUseCase(new Mock<IUserRepository>().Object, new Mock<IListingRepository>().Object);
            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };

            var result = await useCase.ExecuteAsync(new SearchUsersWithListingsRequest("Іван"), auth);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task BlockUser_WhenAdmin_BlocksSuccessfully()
        {
            var mockUsers = new Mock<IUserRepository>();
            var useCase = new BlockUserUseCase(mockUsers.Object, new Mock<IAuditLogGateway>().Object);
            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Admin };

            var result = await useCase.ExecuteAsync(new BlockUserRequest(Guid.NewGuid(), true), auth);

            Assert.True(result.IsSuccess);
            mockUsers.Verify(r => r.SetBlockedAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BlockUser_WhenGuest_ReturnsFailure()
        {
            var useCase = new BlockUserUseCase(new Mock<IUserRepository>().Object, new Mock<IAuditLogGateway>().Object);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await useCase.ExecuteAsync(new BlockUserRequest(Guid.NewGuid(), true), auth);

            Assert.False(result.IsSuccess);
        }
    }
}