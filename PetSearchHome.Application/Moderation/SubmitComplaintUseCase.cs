using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record SubmitComplaintRequest(Guid ListingId, string Reason);

    public class SubmitComplaintUseCase : IUseCase<SubmitComplaintRequest, Guid>
    {
        private readonly IComplaintRepository _complaints;
        private readonly IListingRepository _listings;
        private readonly IAuditLogGateway _audit;

        public SubmitComplaintUseCase(
            IComplaintRepository complaints,
            IListingRepository listings,
            IAuditLogGateway audit)
        {
            _complaints = complaints;
            _listings = listings;
            _audit = audit;
        }

        public async Task<Guid> ExecuteAsync(SubmitComplaintRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                throw new UnauthorizedAccessException("Authentication required.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new InvalidOperationException("Complaint reason is required.");
            }

            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing is null)
            {
                throw new InvalidOperationException("Listing not found.");
            }

            var complaint = new Complaint
            {
                ListingId = request.ListingId,
                ReporterId = authContext.UserId.Value,
                Reason = request.Reason.Trim(),
                Status = "pending",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _complaints.AddAsync(complaint, cancellationToken);
            await _audit.RecordAsync("submit_complaint", authContext.UserId.Value, complaint.Id.ToString(), cancellationToken);

            return complaint.Id;
        }
    }
}
