using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Models.Admin
{
    public class AdminDashboardViewModel
    {
        public IReadOnlyList<PetListing> PendingListings { get; set; } = new List<PetListing>();

        public IReadOnlyList<Complaint> OpenComplaints { get; set; } = new List<Complaint>();
        public IReadOnlyList<Tag> Tags { get; set; } = new List<Tag>();
        public IReadOnlyList<Category> Categories { get; set; } = new List<Category>();
    }
}
