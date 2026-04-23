namespace PetSearchHome_WEB.Domain.Entities
{
    public class ChatConversation
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid UserAId { get; init; }
        public Guid UserBId { get; init; }
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        public bool HasParticipant(Guid userId) => userId == UserAId || userId == UserBId;

        public Guid GetOtherParticipant(Guid userId)
        {
            if (userId == UserAId)
            {
                return UserBId;
            }

            if (userId == UserBId)
            {
                return UserAId;
            }

            throw new InvalidOperationException("User is not part of conversation.");
        }
    }
}
