using System.ComponentModel.DataAnnotations;

namespace PetSearchHome_WEB.Models.Auth
{
    public class RegisterIndividualViewModel
    {
        [Required(ErrorMessage = "Введіть Ваше ім'я та прізвище")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити мінімум 6 символів")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}