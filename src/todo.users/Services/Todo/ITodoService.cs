namespace todo.users.Services.Todo;

public interface ITodoService
{
    Task<db.Todo> CreateTodo(db.Todo todo);
    IEnumerable<db.Todo> FindAllTodos(Guid userId);
    Task<db.Todo> MarkTodoCompleted(Guid todoExternalId, Guid userId);
}