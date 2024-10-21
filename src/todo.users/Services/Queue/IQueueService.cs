namespace todo.users.Services.Queue;

public interface IQueueService
{
    Task<bool> AddCreateUserReqToQueue(model.User user);
}