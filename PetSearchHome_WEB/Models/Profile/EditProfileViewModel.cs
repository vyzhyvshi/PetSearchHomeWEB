using System.ComponentModel.DataAnnotations;

namespace PetSearchHome_WEB.Models.Profile
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Ім'я або назва є обов'язковими")]
        public string DisplayName { get; set; } = string.Empty;

        // Поля тільки для притулків
        public bool IsShelter { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
    }
}