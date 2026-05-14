using System.ComponentModel.DataAnnotations;

namespace PetSearchHome_WEB.Models.Listing
{
    public class CreateListingViewModel
    {
        [Required(ErrorMessage = "Введіть заголовок оголошення")]
        [Display(Name = "Заголовок")]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть вид тварини")]
        [Display(Name = "Вид тварини")]
        [MaxLength(50)]
        public string AnimalType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть місто або локацію")]
        [Display(Name = "Локація")]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Опис")]
        [MaxLength(2000, ErrorMessage = "Опис не може перевищувати 2000 символів")]
        public string? Description { get; set; }

        [Display(Name = "Фото (кожне посилання з нового рядка)")]
        public string? PhotoUrlsText { get; set; }

        [Display(Name = "Терміновий пошук")]
        public bool IsUrgent { get; set; }
    }
}
