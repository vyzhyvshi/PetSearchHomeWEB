using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<IReadOnlyList<Notification>> GetByRecipientIdAsync(int recipientId, CancellationToken cancellationToken = default);

        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
    }
}