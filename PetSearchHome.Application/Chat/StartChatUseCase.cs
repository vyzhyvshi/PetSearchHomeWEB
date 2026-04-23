using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record StartChatRequest(Guid OtherUserId);

    public class StartChatUseCase : IUseCase<StartChatRequest, Result<ChatConversation>>
    {
        private readonly IUserRepository _users;
        private readonly IChatRepository _chats;

        public StartChatUseCase(IUserRepository users, IChatRepository chats)
        {
            _users = users;
            _chats = chats;
        }

        public async Task<Result<ChatConversation>> ExecuteAsync(StartChatRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<ChatConversation>("Потрібна авторизація.");
            }

            if (authContext.UserId == request.OtherUserId)
            {
                return Result.Failure<ChatConversation>("Неможливо створити чат із самим собою.");
            }

            var otherUser = await _users.GetByIdAsync(request.OtherUserId, cancellationToken);
            if (otherUser is null)
            {
                return Result.Failure<ChatConversation>("Користувача не знайдено.");
            }

            var existing = await _chats.GetConversationAsync(authContext.UserId.Value, request.OtherUserId, cancellationToken);
            if (existing is not null)
            {
                return Result.Success(existing);
            }

            var created = await _chats.CreateConversationAsync(authContext.UserId.Value, request.OtherUserId, cancellationToken);
            return Result.Success(created);
        }
    }
}
