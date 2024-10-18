using todo.users.model;

namespace todo.users.Clients;

public interface IStorageServiceClient
{
    Task<bool> RequestCreateUser(User user);
    Task<User> GetUser(Guid userId);
    Task<List<Todo>> GetAllTodos(Guid userId);
    Task<bool> RequestCreateTodo(Guid userId, Todo todo);
    Task<bool> RequestMarkTodoCompleted(Guid userId, Guid todoId);
   
}