namespace todo.users.Services.User;

public interface IUserService
{ 
    Task<db.User> FindUser(Guid externalId);
    Task<db.User> CreateUser(db.User user);
    Task<bool> DeleteUser(Guid externalId);
}