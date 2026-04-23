using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Chat
{
    public sealed record ListChatConversationsRequest;

    public sealed record ChatConversationSummary(ChatConversation Conversation, ChatMessage? LastMessage);

    public class ListChatConversationsUseCase : IUseCase<ListChatConversationsRequest, Result<IReadOnlyList<ChatConversationSummary>>>
    {
        private readonly IChatRepository _chats;

        public ListChatConversationsUseCase(IChatRepository chats)
        {
            _chats = chats;
        }

        public async Task<Result<IReadOnlyList<ChatConversationSummary>>> ExecuteAsync(ListChatConversationsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<IReadOnlyList<ChatConversationSummary>>("Потрібна авторизація.");
            }

            var conversations = await _chats.ListConversationsAsync(authContext.UserId.Value, cancellationToken);
            var summaries = new List<ChatConversationSummary>(conversations.Count);

            foreach (var conversation in conversations)
            {
                var lastMessage = await _chats.GetLastMessageAsync(conversation.Id, cancellationToken);
                summaries.Add(new ChatConversationSummary(conversation, lastMessage));
            }

            return Result.Success<IReadOnlyList<ChatConversationSummary>>(summaries);
        }
    }
}
