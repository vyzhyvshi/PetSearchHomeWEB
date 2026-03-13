using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record BlockUserRequest(Guid UserId, bool Block);

    public class BlockUserUseCase : IUseCase<BlockUserRequest, bool>
    {
        private readonly IUserRepository _users;
        private readonly IAuditLogGateway _audit;

        public BlockUserUseCase(IUserRepository users, IAuditLogGateway audit)
        {
            _users = users;
            _audit = audit;
        }

        public async Task<bool> ExecuteAsync(BlockUserRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                throw new UnauthorizedAccessException("Admin role required.");
            }

            await _users.SetBlockedAsync(request.UserId, request.Block, cancellationToken);
            await _audit.RecordAsync(request.Block ? "block_user" : "unblock_user", authContext.UserId ?? Guid.Empty, request.UserId.ToString(), cancellationToken);
            return true;
        }
    }
}
