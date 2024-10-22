namespace todo.users.Services.Queue;

public interface IQueueService
{
    Task<bool> AddCreateUserReqToQueue(model.User user);
    Task<bool> AddCreateTodoReqToQueue(Guid userId, model.Todo todo);
}