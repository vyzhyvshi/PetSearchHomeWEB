using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatConversation?> GetConversationAsync(int userId, int otherUserId, CancellationToken cancellationToken = default);
        Task<ChatConversation?> GetConversationByIdAsync(int conversationId, CancellationToken cancellationToken = default);
        Task<ChatConversation> CreateConversationAsync(int userId, int otherUserId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChatConversation>> ListConversationsAsync(int userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChatMessage>> ListMessagesAsync(int conversationId, int? viewerUserId = null, CancellationToken cancellationToken = default); Task<int> CountUnreadMessagesAsync(int conversationId, int userId, CancellationToken cancellationToken = default);
        Task MarkMessagesReadAsync(int conversationId, int userId, CancellationToken cancellationToken = default);
        Task ClearHistoryAsync(int conversationId, int userId, DateTimeOffset clearedAt, CancellationToken cancellationToken = default);
        Task<ChatMessage?> GetLastMessageAsync(int conversationId, int? viewerUserId = null, CancellationToken cancellationToken = default); Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
        Task UpdateMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
        Task<ChatMessage?> GetMessageByIdAsync(int messageId, CancellationToken cancellationToken = default);
        Task DeleteMessageAsync(int messageId, CancellationToken cancellationToken = default);
        Task<bool> IsBlockedAsync(int blockerId, int blockedId, CancellationToken cancellationToken = default);
        Task SetBlockedAsync(int blockerId, int blockedId, bool isBlocked, CancellationToken cancellationToken = default);
    }
}
