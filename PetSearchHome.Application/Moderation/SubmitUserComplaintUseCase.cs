using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record SubmitUserComplaintRequest(Guid TargetUserId, string Reason);

    public class SubmitUserComplaintUseCase : IUseCase<SubmitUserComplaintRequest, Result<Guid>>
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

        public async Task<Result<Guid>> ExecuteAsync(SubmitUserComplaintRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<Guid>("Необхідна авторизація.");
            }

            if (authContext.UserId.Value == request.TargetUserId)
            {
                return Result.Failure<Guid>("Не можна надсилати скаргу на власний профіль.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Result.Failure<Guid>("Причина скарги є обов'язковою.");
            }

            var targetUser = await _users.GetByIdAsync(request.TargetUserId, cancellationToken);
            if (targetUser is null)
            {
                return Result.Failure<Guid>("Користувача не знайдено.");
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
            await _audit.RecordAsync("submit_user_complaint", authContext.UserId.Value, complaint.Id.ToString(), cancellationToken);

            return complaint.Id;
        }
    }
}
