namespace PetSearchHome_WEB.Models.Listing
{
    public class CatalogViewModel
    {
        public ListingFilterViewModel Filter { get; set; } = new ListingFilterViewModel();
        public IReadOnlyList<ListingSummaryViewModel> Results { get; set; } = new List<ListingSummaryViewModel>();
    }
}