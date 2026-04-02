namespace PetSearchHome_WEB.Security
{
    public static class RoleNames
    {
        public const string Person = "Person";
        public const string Shelter = "Shelter";
        public const string Admin = "Admin";

        public const string AuthenticatedUser = Person + "," + Shelter + "," + Admin;
    }
}
