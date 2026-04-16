using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Models.Profile
{
    public class UserSearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public IReadOnlyList<User> Results { get; set; } = new List<User>();
    }
}
