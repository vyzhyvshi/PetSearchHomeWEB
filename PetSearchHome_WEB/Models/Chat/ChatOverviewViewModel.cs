namespace PetSearchHome_WEB.Models.Chat
{
    public class ChatOverviewViewModel
    {
        public IReadOnlyList<ChatConversationItemViewModel> Conversations { get; init; } = Array.Empty<ChatConversationItemViewModel>();
    }

    public class ChatConversationItemViewModel
    {
        public Guid ConversationId { get; init; }
        public Guid OtherUserId { get; init; }
        public string OtherDisplayName { get; init; } = string.Empty;
        public string? LastMessage { get; init; }
        public DateTimeOffset? LastMessageAt { get; init; }
    }
}
