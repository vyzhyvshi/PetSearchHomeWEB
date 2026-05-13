using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record SubmitUserComplaintRequest(int TargetUserId, string Reason);

    public class SubmitUserComplaintUseCase : IUseCase<SubmitUserComplaintRequest, Result<int>>
    {
        private readonly IComplaintRepository _complaints;
        private readonly IUserRepository _users;
        private readonly IAuditLogGateway _audit;

        public SubmitUserComplaintUseCase(
            IComplaintRepository complaints,
            IUserRepository users,
            IAuditLogGateway audit)
        {
            _complaints = complaints;
            _users = users;
            _audit = audit;
        }

        public async Task<Result<int>> ExecuteAsync(SubmitUserComplaintRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<int>("Необхідна авторизація.");
            }

            if (authContext.UserId.Value == request.TargetUserId)
            {
                return Result.Failure<int>("Не можна надсилати скаргу на власний профіль.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Result.Failure<int>("Причина скарги є обов'язковою.");
            }

            var targetUser = await _users.GetByIdAsync(request.TargetUserId, cancellationToken); if (targetUser is null)
            {
                return Result.Failure<int>("Користувача не знайдено.");
            }

            Complaint complaint = new()
            {
                ReportedType = ReportedEntityType.User,
                ReportedEntityId = request.TargetUserId,
                ReporterId = authContext.UserId.Value,
                Reason = request.Reason.Trim(),
                Status = "pending",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _complaints.AddAsync(complaint, cancellationToken);
            await _audit.RecordAsync("submit_user_complaint", authContext.UserId.Value, complaint.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), cancellationToken);
            complaint.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return complaint.Id;
        }
    }
}
