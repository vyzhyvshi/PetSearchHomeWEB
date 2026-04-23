using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record UnblockChatUserRequest(Guid OtherUserId);

    public class UnblockChatUserUseCase : IUseCase<UnblockChatUserRequest, Result>
    {
        private readonly IChatRepository _chats;

        public UnblockChatUserUseCase(IChatRepository chats)
        {
            _chats = chats;
        }

        public async Task<Result> ExecuteAsync(UnblockChatUserRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure("Потрібна авторизація.");
            }

            await _chats.SetBlockedAsync(authContext.UserId.Value, request.OtherUserId, false, cancellationToken);
            return Result.Success();
        }
    }
}
