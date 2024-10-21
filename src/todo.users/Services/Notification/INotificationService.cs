using Microsoft.AspNetCore.Http;
using todo.users.model.Notification;

namespace todo.users.Services.Notification;

public interface INotificationService
{
    public Task ConfirmSubscription(SnsMessage message);
    public Task StartWebSocket(HttpContext context);
    public Task BroadcastMessage(SnsMessage message);
}