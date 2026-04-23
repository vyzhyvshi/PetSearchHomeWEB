using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record GetChatThreadRequest(Guid ConversationId);

    public sealed record ChatThread(ChatConversation Conversation, IReadOnlyList<ChatMessage> Messages);

    public class GetChatThreadUseCase : IUseCase<GetChatThreadRequest, Result<ChatThread>>
    {
        private readonly IChatRepository _chats;

        public GetChatThreadUseCase(IChatRepository chats)
        {
            _chats = chats;
        }

        public async Task<Result<ChatThread>> ExecuteAsync(GetChatThreadRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<ChatThread>("Потрібна авторизація.");
            }

            var conversation = await _chats.GetConversationByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null)
            {
                return Result.Failure<ChatThread>("Чат не знайдено.");
            }

            if (!conversation.HasParticipant(authContext.UserId.Value))
            {
                return Result.Failure<ChatThread>("Немає доступу до чату.");
            }

            var messages = await _chats.ListMessagesAsync(conversation.Id, cancellationToken);
            return Result.Success(new ChatThread(conversation, messages));
        }
    }
}
