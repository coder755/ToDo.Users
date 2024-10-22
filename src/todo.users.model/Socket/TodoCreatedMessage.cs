namespace todo.users.model.Socket;

public class TodoCreatedMessage
{
    public IncomingMessageType Type = IncomingMessageType.TodoCreated;
    public Guid UserId { get; set; }
}