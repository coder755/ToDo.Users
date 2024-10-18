using todo.users.model;

namespace todo.users.Clients;

public interface IStorageServiceClient
{
    public Task<bool> RequestCreateUser(User user);
    public Task<User> GetUser(Guid userId);
}