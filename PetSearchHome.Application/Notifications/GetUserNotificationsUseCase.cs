using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Notifications
{
    public sealed record GetUserNotificationsRequest;

    public sealed record NotificationDto(
    int Id,
    string Message,
    bool IsRead,
    DateTimeOffset CreatedAt);

    public class GetUserNotificationsUseCase : IUseCase<GetUserNotificationsRequest, Result<IReadOnlyList<NotificationDto>>>
    {
        private readonly INotificationRepository _notifications;

        public GetUserNotificationsUseCase(INotificationRepository notifications)
        {
            _notifications = notifications;
        }

        public async Task<Result<IReadOnlyList<NotificationDto>>> ExecuteAsync(GetUserNotificationsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<IReadOnlyList<NotificationDto>>("Потрібна авторизація для перегляду сповіщень.");
            }

            var userNotifications = await _notifications.GetByRecipientIdAsync(authContext.UserId.Value, cancellationToken);

            var dtoList = userNotifications.Select(n => new NotificationDto(
                n.Id,
                n.Message,
                n.IsRead,
                n.CreatedAt
            )).ToList();

            return Result.Success<IReadOnlyList<NotificationDto>>(dtoList);
        }
    }
}