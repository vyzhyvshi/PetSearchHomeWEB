namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface INotificationGateway
    {
        Task NotifyAsync(Guid recipientId, string message, CancellationToken cancellationToken = default);
    }
}
