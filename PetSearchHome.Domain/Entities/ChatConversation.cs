namespace PetSearchHome_WEB.Domain.Entities
{
    public class ChatConversation
    {
        public int Id { get; init; }
        public int UserAId { get; init; }
        public int UserBId { get; init; }
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        public bool HasParticipant(int userId) => userId == UserAId || userId == UserBId;

        public int GetOtherParticipant(int userId)
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
