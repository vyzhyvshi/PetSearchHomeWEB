using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record SendChatMessageRequest(Guid ConversationId, string Message);

    public class SendChatMessageUseCase : IUseCase<SendChatMessageRequest, Result>
    {
        private readonly IChatRepository _chats;

        public SendChatMessageUseCase(IChatRepository chats)
        {
            _chats = chats;
        }

        public async Task<Result> ExecuteAsync(SendChatMessageRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure("Потрібна авторизація.");
            }

            var conversation = await _chats.GetConversationByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null)
            {
                return Result.Failure("Чат не знайдено.");
            }

            if (!conversation.HasParticipant(authContext.UserId.Value))
            {
                return Result.Failure("Немає доступу до чату.");
            }

            var messageText = request.Message?.Trim();
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return Result.Failure("Повідомлення не може бути порожнім.");
            }

            var message = new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderId = authContext.UserId.Value,
                Content = messageText,
                SentAt = DateTimeOffset.UtcNow
            };

            await _chats.AddMessageAsync(message, cancellationToken);
            return Result.Success();
        }
    }
}
