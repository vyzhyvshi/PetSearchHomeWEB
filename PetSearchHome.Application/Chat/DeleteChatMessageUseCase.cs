using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record DeleteChatMessageRequest(Guid MessageId);

    public class DeleteChatMessageUseCase : IUseCase<DeleteChatMessageRequest, Result<ChatMessage>>
    {
        private readonly IChatRepository _chats;

        public DeleteChatMessageUseCase(IChatRepository chats)
        {
            _chats = chats;
        }

        public async Task<Result<ChatMessage>> ExecuteAsync(DeleteChatMessageRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<ChatMessage>("Потрібна авторизація.");
            }

            var message = await _chats.GetMessageByIdAsync(request.MessageId, cancellationToken);
            if (message is null)
            {
                return Result.Failure<ChatMessage>("Повідомлення не знайдено.");
            }

            var conversation = await _chats.GetConversationByIdAsync(message.ConversationId, cancellationToken);
            if (conversation is null || !conversation.HasParticipant(authContext.UserId.Value))
            {
                return Result.Failure<ChatMessage>("Немає доступу до чату.");
            }

            if (message.SenderId != authContext.UserId.Value)
            {
                return Result.Failure<ChatMessage>("Ви можете видалити лише свої повідомлення.");
            }

            await _chats.DeleteMessageAsync(message.Id, cancellationToken);
            return Result.Success(message);
        }
    }
}
