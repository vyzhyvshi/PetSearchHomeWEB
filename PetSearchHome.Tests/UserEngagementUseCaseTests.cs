using Microsoft.Extensions.Options;
using Moq;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Application.Profiles;
using PetSearchHome_WEB.Application.Reviews;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class UserEngagementUseCaseTests
    {
        [Fact]
        public async Task SubmitComplaint_WhenAuthorized_ReturnsSuccess()
        {
            var listingsMock = new Mock<IListingRepository>();
            listingsMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PetListing { Id = Guid.NewGuid() });

            var complaintsMock = new Mock<IComplaintRepository>();
            var options = Options.Create(new ModerationSettings());
            var useCase = new SubmitComplaintUseCase(complaintsMock.Object, listingsMock.Object, new Mock<IAuditLogGateway>().Object, options);

            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };
            var result = await useCase.ExecuteAsync(new SubmitComplaintRequest(Guid.NewGuid(), "Спам"), auth);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SubmitComplaint_WhenGuest_ReturnsFailure()
        {
            var options = Options.Create(new ModerationSettings());
            var useCase = new SubmitComplaintUseCase(new Mock<IComplaintRepository>().Object, new Mock<IListingRepository>().Object, new Mock<IAuditLogGateway>().Object, options);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await useCase.ExecuteAsync(new SubmitComplaintRequest(Guid.NewGuid(), "Спам"), auth);

            Assert.False(result.IsSuccess);
            Assert.Contains("авторизація", result.ErrorMessage);
        }

        [Fact]
        public async Task SubmitUserComplaint_WhenAuthorized_ReturnsSuccess()
        {
            var complaintsMock = new Mock<IComplaintRepository>();
            var usersMock = new Mock<IUserRepository>();
            usersMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid() });

            var useCase = new SubmitUserComplaintUseCase(complaintsMock.Object, usersMock.Object, new Mock<IAuditLogGateway>().Object);

            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };
            var result = await useCase.ExecuteAsync(new SubmitUserComplaintRequest(Guid.NewGuid(), "Грубість"), auth);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SubmitUserComplaint_WhenGuest_ReturnsFailure()
        {
            var useCase = new SubmitUserComplaintUseCase(new Mock<IComplaintRepository>().Object, new Mock<IUserRepository>().Object, new Mock<IAuditLogGateway>().Object);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await useCase.ExecuteAsync(new SubmitUserComplaintRequest(Guid.NewGuid(), "Грубість"), auth);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task LeaveReview_WhenAuthorized_ReturnsSuccess()
        {
            var reviewsMock = new Mock<IReviewRepository>();
            var useCase = new LeaveReviewUseCase(reviewsMock.Object);

            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };
            var result = await useCase.ExecuteAsync(new LeaveReviewRequest(Guid.NewGuid(), (byte)5, "Клас!", true), auth);

            Assert.NotEqual(Guid.Empty, result);
            reviewsMock.Verify(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LeaveReview_WhenGuest_ReturnsFailure()
        {
            var useCase = new LeaveReviewUseCase(new Mock<IReviewRepository>().Object);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                useCase.ExecuteAsync(new LeaveReviewRequest(Guid.NewGuid(), (byte)5, "Клас!", true), auth));
        }

        [Fact]
        public async Task ViewOrgStats_WhenAllowed_ReturnsStats()
        {
            var statsMock = new Mock<IOrgStatsRepository>();
            statsMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OrgStats());

            var useCase = new ViewOrgStatsUseCase(statsMock.Object);

            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Admin };

            var result = await useCase.ExecuteAsync(new ViewOrgStatsRequest(Guid.NewGuid()), auth);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ViewOrgStats_WhenNotAllowed_ThrowsException()
        {
            var statsMock = new Mock<IOrgStatsRepository>();
            var useCase = new ViewOrgStatsUseCase(statsMock.Object);

            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                useCase.ExecuteAsync(new ViewOrgStatsRequest(Guid.NewGuid()), auth));
        }

        [Fact]
        public async Task SearchPublicUsers_WithValidQuery_ReturnsUsers()
        {
            var usersMock = new Mock<IUserRepository>();
            usersMock.Setup(r => r.SearchAsync("Притулок", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { new User { DisplayName = "Притулок Надія" } });

            var listingsMock = new Mock<IListingRepository>();
            listingsMock.Setup(r => r.ListByOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing> { new PetListing() });

            var useCase = new SearchPublicUsersWithListingsUseCase(usersMock.Object, listingsMock.Object);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await useCase.ExecuteAsync(new SearchPublicUsersWithListingsRequest("Притулок"), auth);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SearchPublicUsers_WhenEmptyQuery_ReturnsEmptyList()
        {
            var usersMock = new Mock<IUserRepository>();
            usersMock.Setup(r => r.SearchAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>());

            var listingsMock = new Mock<IListingRepository>();
            listingsMock.Setup(r => r.ListByOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing>());

            var useCase = new SearchPublicUsersWithListingsUseCase(usersMock.Object, listingsMock.Object);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await useCase.ExecuteAsync(new SearchPublicUsersWithListingsRequest(string.Empty), auth);

            Assert.True(result.IsSuccess);
        }
    }
}