using Microsoft.Extensions.Options;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record SubmitComplaintRequest(Guid ListingId, string Reason);

    public class SubmitComplaintUseCase : IUseCase<SubmitComplaintRequest, Result<Guid>>
    {
        private readonly IComplaintRepository _complaints;
        private readonly IListingRepository _listings;
        private readonly IAuditLogGateway _audit;
        private readonly ModerationSettings _settings;

        public SubmitComplaintUseCase(
            IComplaintRepository complaints,
            IListingRepository listings,
            IAuditLogGateway audit,
            IOptions<ModerationSettings> options)
        {
            _complaints = complaints;
            _listings = listings;
            _audit = audit;
            _settings = options.Value;
        }

        public async Task<Result<Guid>> ExecuteAsync(SubmitComplaintRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<Guid>("Необхідна авторизація.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Result.Failure<Guid>("Причина скарги є обов'язковою.");
            }

            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing is null)
            {
                return Result.Failure<Guid>("Оголошення не знайдено.");
            }

            Complaint complaint = new()
            {
                ReportedType = ReportedEntityType.Listing,
                ReportedEntityId = request.ListingId,
                ReporterId = authContext.UserId.Value,
                Reason = request.Reason.Trim(),
                Status = "pending",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var pendingComplaints = await _complaints.CountPendingComplaintsForEntityAsync(request.ListingId, cancellationToken);

            await _complaints.AddAsync(complaint, cancellationToken);
            await _audit.RecordAsync("submit_complaint", authContext.UserId.Value, complaint.Id.ToString(), cancellationToken);

            if (pendingComplaints + 1 >= _settings.ComplaintsThresholdForAutoHide && listing.Status != ListingStatus.PendingModeration)
            {
                var updatedListing = listing with { Status = ListingStatus.PendingModeration };
                await _listings.UpdateAsync(updatedListing, cancellationToken);
                await _audit.RecordAsync("auto_hide_by_reports", Guid.Empty, listing.Id.ToString(), cancellationToken);
            }

            return complaint.Id;
        }
    }
}
