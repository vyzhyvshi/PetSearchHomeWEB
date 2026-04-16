using System.ComponentModel.DataAnnotations;

namespace PetSearchHome_WEB.Models.Auth
{
    public class RegisterShelterViewModel
    {
        [Required(ErrorMessage = "Введіть назву притулку")]
        public string ShelterName { get; set; } = string.Empty;

        [Display(Name = "Опис притулку")]
        [MaxLength(1000, ErrorMessage = "Опис не може перевищувати 1000 символів")]
        public string? Description { get; set; }

        [Display(Name = "Сайт")]
        [Url(ErrorMessage = "Вкажіть коректне посилання")]
        public string? Website { get; set; }

        [Display(Name = "Адреса")]
        [MaxLength(250, ErrorMessage = "Адреса не може перевищувати 250 символів")]
        public string? Address { get; set; }

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
