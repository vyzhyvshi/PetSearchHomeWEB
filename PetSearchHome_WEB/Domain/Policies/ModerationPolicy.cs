using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Policies
{
    public static class ModerationPolicy
    {
        public static bool RequiresModeration(Role role, bool isShelterTrusted = false)
        {
            if (role == Role.Admin)
            {
                return false;
            }

            if (role == Role.Shelter && isShelterTrusted)
            {
                return false;
            }

            return role == Role.Person || role == Role.Shelter;
        }
    }
}
