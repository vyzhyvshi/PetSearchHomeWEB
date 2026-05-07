using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PetSearchHome_WEB.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub
{
}
