using Microsoft.Extensions.Options;
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
        private readonly IOptions<ModerationSettings> _options; 
        private readonly SubmitComplaintUseCase _useCase;

        public SubmitComplaintUseCaseTests()
        {
            _complaintsMock = new Mock<IComplaintRepository>();
            _listingsMock = new Mock<IListingRepository>();
            _auditMock = new Mock<IAuditLogGateway>();

            var settings = new ModerationSettings { ComplaintsThresholdForAutoHide = 3 };
            _options = Options.Create(settings);

            _useCase = new SubmitComplaintUseCase(
                _complaintsMock.Object,
                _listingsMock.Object,
                _auditMock.Object,
                _options); 
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
                .ReturnsAsync(new PetListing { Id = listingId, Status = ListingStatus.Published });

            _complaintsMock
                .Setup(repo => repo.CountPendingComplaintsForEntityAsync(listingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            _complaintsMock.Verify(repo => repo.AddAsync(
                It.Is<Complaint>(complaint =>
                    complaint.ReportedType == ReportedEntityType.Listing &&
                    complaint.ReportedEntityId == listingId),
                It.IsAny<CancellationToken>()), Times.Once);
                
            _listingsMock.Verify(repo => repo.UpdateAsync(It.IsAny<PetListing>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenThresholdReached_ChangesListingStatusToPendingModeration()
        {
            var userId = Guid.NewGuid();
            var listingId = Guid.NewGuid();
            var authContext = new AuthContext { UserId = userId, Role = Role.Person };
            var request = new SubmitComplaintRequest(listingId, "Жорстоке поводження");

            var existingListing = new PetListing { Id = listingId, Status = ListingStatus.Published };

            _listingsMock
                .Setup(repo => repo.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingListing);

            _complaintsMock
                .Setup(repo => repo.CountPendingComplaintsForEntityAsync(listingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.True(result.IsSuccess);

            _listingsMock.Verify(repo => repo.UpdateAsync(
                It.Is<PetListing>(l => l.Id == listingId && l.Status == ListingStatus.PendingModeration),
                It.IsAny<CancellationToken>()), Times.Once);
                
            _auditMock.Verify(audit => audit.RecordAsync(
                "auto_hide_by_reports", 
                Guid.Empty, 
                listingId.ToString(), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}