using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<IReadOnlyList<Notification>> GetByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken = default);

        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    }
}