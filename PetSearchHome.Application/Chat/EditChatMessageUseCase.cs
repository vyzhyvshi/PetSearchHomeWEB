using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record EditChatMessageRequest(Guid MessageId, string NewContent);

    public class EditChatMessageUseCase : IUseCase<EditChatMessageRequest, Result>
    {
        private readonly IChatRepository _chats;

        public EditChatMessageUseCase(IChatRepository chats)
        {
            _chats = chats;
        }

        public async Task<Result> ExecuteAsync(EditChatMessageRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null) return Result.Failure("Потрібна авторизація.");

            var message = await _chats.GetMessageByIdAsync(request.MessageId, cancellationToken);
            if (message is null) return Result.Failure("Повідомлення не знайдено.");

            if (message.SenderId != authContext.UserId.Value)
            {
                return Result.Failure("Ви можете редагувати лише свої повідомлення.");
            }

            var newText = request.NewContent?.Trim();
            if (string.IsNullOrWhiteSpace(newText))
            {
                return Result.Failure("Текст повідомлення не може бути порожнім.");
            }

            message.Content = newText;

            await _chats.UpdateMessageAsync(message, cancellationToken);

            return Result.Success();
        }
    }
}