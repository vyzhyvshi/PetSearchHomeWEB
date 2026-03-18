using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Policies
{
    public static class ListingAccessPolicy
    {
        public static bool CanCreate(Role role)
        {
            return role == Role.Person || role == Role.Shelter || role == Role.Admin;
        }

        public static bool CanManage(Role role, Guid ownerId, Guid? actorId)
        {
            if (role == Role.Admin)
            {
                return true;
            }

            return actorId.HasValue
                && (role == Role.Person || role == Role.Shelter)
                && actorId.Value == ownerId;
        }
    }
}
