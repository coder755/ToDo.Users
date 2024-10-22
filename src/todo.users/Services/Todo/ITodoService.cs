namespace todo.users.Services.Todo;

public interface ITodoService
{
    Task<bool> RequestCreateTodo(Guid userId, model.Todo todo, bool useQueue);
    Task<List<model.Todo>> GetAllTodos(Guid userId);
    Task<bool> RequestMarkTodoCompleted(Guid userId, Guid todoExternalId);
}