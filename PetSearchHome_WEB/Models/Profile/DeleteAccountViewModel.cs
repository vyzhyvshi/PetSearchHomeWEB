using System.ComponentModel.DataAnnotations;

namespace PetSearchHome_WEB.Models.Profile
{
    public class DeleteAccountViewModel
    {
        [Required(ErrorMessage = "Введіть поточний пароль")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
