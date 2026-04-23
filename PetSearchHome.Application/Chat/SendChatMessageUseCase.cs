using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record SendChatMessageRequest(Guid ConversationId, string Message, string? ImageUrl);

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
            if (string.IsNullOrWhiteSpace(messageText) && string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                return Result.Failure("Повідомлення не може бути порожнім.");
            }

            var otherUserId = conversation.GetOtherParticipant(authContext.UserId.Value);
            var blockedByMe = await _chats.IsBlockedAsync(authContext.UserId.Value, otherUserId, cancellationToken);
            var blockedByOther = await _chats.IsBlockedAsync(otherUserId, authContext.UserId.Value, cancellationToken);
            if (blockedByMe || blockedByOther)
            {
                return Result.Failure("Неможливо надіслати повідомлення в заблокований чат.");
            }

            var message = new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderId = authContext.UserId.Value,
                Content = messageText ?? string.Empty,
                ImageUrl = request.ImageUrl,
                SentAt = DateTimeOffset.UtcNow
            };

            await _chats.AddMessageAsync(message, cancellationToken);
            return Result.Success();
        }
    }
}
