using Microsoft.AspNetCore.SignalR;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Hubs;

namespace PetSearchHome_WEB.Infrastructure.Notifications;

public sealed class SignalRNotificationGateway : INotificationGateway
{
    private readonly INotificationRepository _notifications;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public SignalRNotificationGateway(
        INotificationRepository notifications,
        IHubContext<NotificationsHub> hubContext)
    {
        _notifications = notifications;
        _hubContext = hubContext;
    }

    public async Task NotifyAsync(int recipientId, string message, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            RecipientId = recipientId,
            Message = message,
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead = false
        };

        await _notifications.AddAsync(notification, cancellationToken);

        await _hubContext.Clients
                .User(recipientId.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Message,
                notification.CreatedAt,
                notification.IsRead
            }, cancellationToken);
    }
}
