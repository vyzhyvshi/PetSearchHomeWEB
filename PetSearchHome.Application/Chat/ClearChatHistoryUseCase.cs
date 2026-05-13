using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record ClearChatHistoryRequest(int ConversationId);

    public class ClearChatHistoryUseCase : IUseCase<ClearChatHistoryRequest, Result>
    {
        private readonly IChatRepository _chats;

        public ClearChatHistoryUseCase(IChatRepository chats)
        {
            _chats = chats;
        }

        public async Task<Result> ExecuteAsync(ClearChatHistoryRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure("Потрібна авторизація.");
            }

            var conversation = await _chats.GetConversationByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null || !conversation.HasParticipant(authContext.UserId.Value))
            {
                return Result.Failure("Немає доступу до чату.");
            }

            await _chats.ClearHistoryAsync(conversation.Id, authContext.UserId.Value, DateTimeOffset.UtcNow, cancellationToken);
            return Result.Success();
        }
    }
}
