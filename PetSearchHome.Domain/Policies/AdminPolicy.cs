using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Policies
{
    public static class AdminPolicy
    {
        public static bool IsAdmin(Role role) => role == Role.Admin;
    }
}
