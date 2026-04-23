using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatConversation?> GetConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default);
        Task<ChatConversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
        Task<ChatConversation> CreateConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChatConversation>> ListConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChatMessage>> ListMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default);
        Task<ChatMessage?> GetLastMessageAsync(Guid conversationId, CancellationToken cancellationToken = default);
        Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
        Task<ChatMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default);
        Task DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default);
        Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default);
        Task SetBlockedAsync(Guid blockerId, Guid blockedId, bool isBlocked, CancellationToken cancellationToken = default);
    }
}
