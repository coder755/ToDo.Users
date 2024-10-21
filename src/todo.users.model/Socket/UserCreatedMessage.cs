namespace todo.users.model.Socket;

public class UserCreatedMessage
{
    public IncomingMessageType Type = IncomingMessageType.UserCreated;
    public Guid UserId { get; set; }
}