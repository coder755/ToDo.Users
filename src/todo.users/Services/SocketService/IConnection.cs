#nullable enable
using todo.users.model.Notification;

namespace todo.users.Services.SocketService;

public interface IConnection
{
    public Task HandleSnsMessage(SnsMessage message);
}