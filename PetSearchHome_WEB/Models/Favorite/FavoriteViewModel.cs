using PetSearchHome_WEB.Models.Listing;

namespace PetSearchHome_WEB.Models.Favorite
{
    public class FavoriteViewModel
    {
        public IReadOnlyList<ListingSummaryViewModel> Items { get; set; } = new List<ListingSummaryViewModel>();
    }
}