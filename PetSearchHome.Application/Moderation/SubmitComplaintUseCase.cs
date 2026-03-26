using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record SubmitComplaintRequest(Guid ListingId, string Reason);

    public class SubmitComplaintUseCase : IUseCase<SubmitComplaintRequest, Result<Guid>>
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
                ListingId = request.ListingId,
                ReporterId = authContext.UserId.Value,
                Reason = request.Reason.Trim(),
                Status = "pending",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _complaints.AddAsync(complaint, cancellationToken);
            await _audit.RecordAsync("submit_complaint", authContext.UserId.Value, complaint.Id.ToString(), cancellationToken);

            return Result.Success(complaint.Id);
        }
    }
}