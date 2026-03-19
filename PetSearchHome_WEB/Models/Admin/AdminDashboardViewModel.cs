using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Models.Admin
{
    public class AdminDashboardViewModel
    {
        public IReadOnlyList<PetListing> PendingListings { get; set; } = new List<PetListing>();

        public IReadOnlyList<Complaint> OpenComplaints { get; set; } = new List<Complaint>();
    }
}