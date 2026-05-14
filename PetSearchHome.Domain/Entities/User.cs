using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Entities
{
    public class User
    {
        public int Id { get; init; } 
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string PasswordHash { get; init; } = string.Empty;
        public Role Role { get; init; } = Role.Guest;
        public bool IsBlocked { get; init; }
            = false;
        public bool IsDeleted { get; init; }
            = false;
    }
}
