using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record HandleComplaintRequest(Guid ComplaintId, string Resolution);

    public class HandleComplaintUseCase : IUseCase<HandleComplaintRequest, Result<bool>>
    {
        private readonly IComplaintRepository _complaints;
        private readonly IAuditLogGateway _audit;

        public HandleComplaintUseCase(IComplaintRepository complaints, IAuditLogGateway audit)
        {
            _complaints = complaints;
            _audit = audit;
        }

        public async Task<Result<bool>> ExecuteAsync(HandleComplaintRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                return Result.Failure<bool>("Немає прав доступу. Потрібна роль Адміністратора.");
            }

            await _complaints.UpdateStatusAsync(request.ComplaintId, request.Resolution, cancellationToken);
            await _audit.RecordAsync("resolve_complaint", authContext.UserId ?? Guid.Empty, request.ComplaintId.ToString(), cancellationToken);
            return Result.Success(true);
        }
    }
}