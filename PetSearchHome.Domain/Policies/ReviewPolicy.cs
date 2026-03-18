using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Policies
{
    public static class ReviewPolicy
    {
        public static bool CanLeaveReview(Role role, bool hasInteraction)
        {
            if (role == Role.Guest)
            {
                return false;
            }

            return hasInteraction;
        }
    }
}
