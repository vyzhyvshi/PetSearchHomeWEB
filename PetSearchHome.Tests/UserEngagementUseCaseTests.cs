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
        private readonly AuthContext _validAuth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };
        private readonly AuthContext _adminAuth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Admin };

        // Перевіряє, що скарга успішно створюється, якщо оголошення існує
        [Fact]
        public async Task SubmitComplaint_WhenListingExists_ReturnsSuccess()
        {
            var listingsMock = new Mock<IListingRepository>();
            listingsMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PetListing { Id = Guid.NewGuid() });

            var useCase = new SubmitComplaintUseCase(new Mock<IComplaintRepository>().Object, listingsMock.Object, new Mock<IAuditLogGateway>().Object, Options.Create(new ModerationSettings()));

            var result = await useCase.ExecuteAsync(new SubmitComplaintRequest(Guid.NewGuid(), "Шахрайство"), _validAuth);

            Assert.True(result.IsSuccess);
        }

        // Перевіряє, що повертається помилка, якщо оголошення не знайдено
        [Fact]
        public async Task SubmitComplaint_WhenListingNotFound_ReturnsFailure()
        {
            var listingsMock = new Mock<IListingRepository>();
            listingsMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PetListing?)null);

            var useCase = new SubmitComplaintUseCase(new Mock<IComplaintRepository>().Object, listingsMock.Object, new Mock<IAuditLogGateway>().Object, Options.Create(new ModerationSettings()));

            var result = await useCase.ExecuteAsync(new SubmitComplaintRequest(Guid.NewGuid(), "Шахрайство"), _validAuth);

            Assert.False(result.IsSuccess);
            Assert.Contains("не знайдено", result.ErrorMessage);
        }

        // Перевіряє, що скарга на користувача створюється, якщо він існує
        [Fact]
        public async Task SubmitUserComplaint_WhenTargetUserExists_ReturnsSuccess()
        {
            var usersMock = new Mock<IUserRepository>();
            usersMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid() });

            var useCase = new SubmitUserComplaintUseCase(new Mock<IComplaintRepository>().Object, usersMock.Object, new Mock<IAuditLogGateway>().Object);

            var result = await useCase.ExecuteAsync(new SubmitUserComplaintRequest(Guid.NewGuid(), "Грубість"), _validAuth);

            Assert.True(result.IsSuccess);
        }

        // Перевіряє, що не можна поскаржитися на самого себе
        [Fact]
        public async Task SubmitUserComplaint_WhenTargetIsSelf_ReturnsFailure()
        {
            var useCase = new SubmitUserComplaintUseCase(new Mock<IComplaintRepository>().Object, new Mock<IUserRepository>().Object, new Mock<IAuditLogGateway>().Object);

            var request = new SubmitUserComplaintRequest(_validAuth.UserId!.Value, "Грубість");
            var result = await useCase.ExecuteAsync(request, _validAuth);

            Assert.False(result.IsSuccess);
            Assert.Contains("власний профіль", result.ErrorMessage);
        }

        // Перевіряє, що при валідних даних створюється відгук з новим ID
        [Fact]
        public async Task LeaveReview_WithValidData_ReturnsNewReviewId()
        {
            var useCase = new LeaveReviewUseCase(new Mock<IReviewRepository>().Object);

            var result = await useCase.ExecuteAsync(new LeaveReviewRequest(Guid.NewGuid(), (byte)5, "Все чудово!", true), _validAuth);

            Assert.NotEqual(Guid.Empty, result);
        }

        // Перевіряє, що відгук зберігається з правильними даними (рейтинг і коментар)
        [Fact]
        public async Task LeaveReview_WithValidData_SavesCorrectlyToDatabase()
        {
            var reviewsMock = new Mock<IReviewRepository>();
            var useCase = new LeaveReviewUseCase(reviewsMock.Object);

            await useCase.ExecuteAsync(new LeaveReviewRequest(Guid.NewGuid(), (byte)4, "Добре", true), _validAuth);

            reviewsMock.Verify(r => r.AddAsync(It.Is<Review>(rev => rev.Rating == 4 && rev.Comment == "Добре"), It.IsAny<CancellationToken>()), Times.Once);
        }

        // Перевіряє, що статистика повертається, якщо вона існує
        [Fact]
        public async Task ViewOrgStats_WhenStatsExist_ReturnsObject()
        {
            var statsMock = new Mock<IOrgStatsRepository>();
            statsMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OrgStats());

            var useCase = new ViewOrgStatsUseCase(statsMock.Object);

            var result = await useCase.ExecuteAsync(new ViewOrgStatsRequest(Guid.NewGuid()), _adminAuth);

            Assert.NotNull(result);
        }

        // Перевіряє, що повертається null, якщо статистики немає
        [Fact]
        public async Task ViewOrgStats_WhenStatsDoNotExist_ReturnsNull()
        {
            var statsMock = new Mock<IOrgStatsRepository>();
            statsMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrgStats?)null);

            var useCase = new ViewOrgStatsUseCase(statsMock.Object);

            var result = await useCase.ExecuteAsync(new ViewOrgStatsRequest(Guid.NewGuid()), _adminAuth);

            Assert.Null(result);
        }

        // Перевіряє, що пошук повертає список користувачів за потрібним запитом
        [Fact]
        public async Task SearchPublicUsers_WithValidQuery_ReturnsUsersList()
        {
            var usersMock = new Mock<IUserRepository>();
            usersMock.Setup(r => r.SearchAsync("Притулок", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { new User { DisplayName = "Притулок Надія" } });

            var listingsMock = new Mock<IListingRepository>();
            listingsMock.Setup(r => r.ListByOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing> { new PetListing() });

            var options = Options.Create(new SearchSettings { MinQueryLength = 2, MaxResults = 50 });
            var useCase = new SearchPublicUsersWithListingsUseCase(usersMock.Object, listingsMock.Object, options);

            var result = await useCase.ExecuteAsync(new SearchPublicUsersWithListingsRequest("Притулок"), _validAuth);

            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value!);
        }

        // Перевіряє, що при порожньому запиті повертається пустий список без пошуку
        [Fact]
        public async Task SearchPublicUsers_WhenEmptyQuery_ReturnsEmptyListInstantly()
        {
            var options = Options.Create(new SearchSettings { MinQueryLength = 2, MaxResults = 50 });
            var useCase = new SearchPublicUsersWithListingsUseCase(new Mock<IUserRepository>().Object, new Mock<IListingRepository>().Object, options);

            var result = await useCase.ExecuteAsync(new SearchPublicUsersWithListingsRequest("   "), _validAuth);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value!);
        }
    }
}