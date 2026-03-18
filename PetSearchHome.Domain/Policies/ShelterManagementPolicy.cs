using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Policies
{
    public static class ShelterManagementPolicy
    {
        public static bool CanRunBulk(Role role)
        {
            return role == Role.Shelter || role == Role.Admin;
        }
    }
}
