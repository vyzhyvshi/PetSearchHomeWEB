using PetSearchHome_WEB.Models.Listing;

namespace PetSearchHome_WEB.Models.Home
{
    public class HomeIndexViewModel
    {
        public IReadOnlyList<ListingSummaryViewModel> FeaturedListings { get; init; }
            = Array.Empty<ListingSummaryViewModel>();
    }
}
