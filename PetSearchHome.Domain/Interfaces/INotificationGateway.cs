namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface INotificationGateway
    {
        Task NotifyAsync(int recipientId, string message, CancellationToken cancellationToken = default);
    }
}
