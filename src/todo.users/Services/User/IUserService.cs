namespace todo.users.Services.User;

public interface IUserService
{ 
    Task<model.User> FindUser(Guid externalId);
    Task<bool> RequestCreateUser(model.User user);
}