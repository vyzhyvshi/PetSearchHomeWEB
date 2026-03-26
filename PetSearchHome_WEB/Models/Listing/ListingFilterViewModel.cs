namespace PetSearchHome_WEB.Models.Listing
{
    public class ListingFilterViewModel
    {
        public string? SearchQuery { get; set; } 
        public string? AnimalType { get; set; }  
        public string? Location { get; set; }    
        public bool OnlyUrgent { get; set; }    
    }
}