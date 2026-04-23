using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record BlockChatUserRequest(Guid OtherUserId);

    public class BlockChatUserUseCase : IUseCase<BlockChatUserRequest, Result>
    {
        private readonly IChatRepository _chats;

        public BlockChatUserUseCase(IChatRepository chats)
        {
            _chats = chats;
        }

        public async Task<Result> ExecuteAsync(BlockChatUserRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure("Потрібна авторизація.");
            }

            if (authContext.UserId == request.OtherUserId)
            {
                return Result.Failure("Не можна блокувати самого себе.");
            }

            await _chats.SetBlockedAsync(authContext.UserId.Value, request.OtherUserId, true, cancellationToken);
            return Result.Success();
        }
    }
}
