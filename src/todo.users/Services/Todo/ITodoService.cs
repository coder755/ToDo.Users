namespace todo.users.Services.Todo;

public interface ITodoService
{
    Task<db.Todo> CreateTodo(db.Todo todo);
}