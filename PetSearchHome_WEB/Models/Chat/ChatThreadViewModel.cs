namespace PetSearchHome_WEB.Models.Chat
{
    public class ChatThreadViewModel
    {
        public Guid ConversationId { get; init; }
        public Guid OtherUserId { get; init; }
        public string OtherDisplayName { get; init; } = string.Empty;
        public IReadOnlyList<ChatMessageViewModel> Messages { get; init; } = Array.Empty<ChatMessageViewModel>();
        public bool IsBlockedByMe { get; init; }
        public bool IsBlockedByOther { get; init; }
        public bool CanSend => !(IsBlockedByMe || IsBlockedByOther);
        public string Message { get; set; } = string.Empty;
    }

    public class ChatMessageViewModel
    {
        public Guid MessageId { get; init; }
        public Guid SenderId { get; init; }
        public string Content { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public DateTimeOffset SentAt { get; init; }
        public bool IsMine { get; init; }
        public bool CanDelete { get; init; }
    }
}
