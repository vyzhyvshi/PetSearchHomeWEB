using System.ComponentModel.DataAnnotations;

namespace PetSearchHome_WEB.Models.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}