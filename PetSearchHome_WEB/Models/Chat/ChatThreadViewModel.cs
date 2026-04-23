namespace PetSearchHome_WEB.Models.Chat
{
    public class ChatThreadViewModel
    {
        public Guid ConversationId { get; init; }
        public Guid OtherUserId { get; init; }
        public string OtherDisplayName { get; init; } = string.Empty;
        public IReadOnlyList<ChatMessageViewModel> Messages { get; init; } = Array.Empty<ChatMessageViewModel>();
        public string Message { get; set; } = string.Empty;
    }

    public class ChatMessageViewModel
    {
        public Guid SenderId { get; init; }
        public string Content { get; init; } = string.Empty;
        public DateTimeOffset SentAt { get; init; }
        public bool IsMine { get; init; }
    }
}
