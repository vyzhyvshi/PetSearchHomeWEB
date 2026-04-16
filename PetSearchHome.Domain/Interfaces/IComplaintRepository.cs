using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IComplaintRepository
    {
        Task AddAsync(Complaint complaint, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Complaint>> ListOpenAsync(CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
        Task<int> CountPendingComplaintsForEntityAsync(Guid entityId, CancellationToken cancellationToken = default);
    }
}
