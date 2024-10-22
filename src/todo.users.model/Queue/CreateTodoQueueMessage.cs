namespace todo.users.model.Queue;

public class CreateTodoQueueMessage
{
    public Todo Todo { get; set; }
    public Guid UserId { get; set; }
}