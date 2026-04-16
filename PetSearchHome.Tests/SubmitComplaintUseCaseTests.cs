using Moq;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class SubmitComplaintUseCaseTests
    {
        private readonly Mock<IComplaintRepository> _complaintsMock;
        private readonly Mock<IListingRepository> _listingsMock;
        private readonly Mock<IAuditLogGateway> _auditMock;
        private readonly SubmitComplaintUseCase _useCase;

        public SubmitComplaintUseCaseTests()
        {
            _complaintsMock = new Mock<IComplaintRepository>();
            _listingsMock = new Mock<IListingRepository>();
            _auditMock = new Mock<IAuditLogGateway>();

            _useCase = new SubmitComplaintUseCase(
                _complaintsMock.Object,
                _listingsMock.Object,
                _auditMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenReasonIsEmpty_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var authContext = new AuthContext { UserId = userId, Role = Role.Person };
            var request = new SubmitComplaintRequest(Guid.NewGuid(), string.Empty);

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.False(result.IsSuccess);
            Assert.Contains("обов'язковою", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExecuteAsync_WhenValidRequest_ReturnsSuccessWithComplaintId()
        {
            var userId = Guid.NewGuid();
            var listingId = Guid.NewGuid();
            var authContext = new AuthContext { UserId = userId, Role = Role.Person };
            var request = new SubmitComplaintRequest(listingId, "Спам");

            _listingsMock
                .Setup(repo => repo.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PetListing { Id = listingId });

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            _complaintsMock.Verify(repo => repo.AddAsync(
                It.Is<Complaint>(complaint =>
                    complaint.ReportedType == ReportedEntityType.Listing &&
                    complaint.ReportedEntityId == listingId),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
