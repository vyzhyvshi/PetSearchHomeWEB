namespace PetSearchHome_WEB.Models.Chat
{
    public class ChatOverviewViewModel
    {
        public IReadOnlyList<ChatConversationItemViewModel> Conversations { get; init; } = Array.Empty<ChatConversationItemViewModel>();
    }

    public class ChatConversationItemViewModel
    {
        public int ConversationId { get; init; }
        public int OtherUserId { get; init; }
        public string OtherDisplayName { get; init; } = string.Empty;
        public string? LastMessage { get; init; }
        public DateTimeOffset? LastMessageAt { get; init; }
        public int UnreadCount { get; init; }
    }
}
