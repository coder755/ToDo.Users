using todo.users.db;

namespace todo.users.Services.Todo;

public class TodoService : ITodoService
{
    private readonly UsersContext _context;
    
    public TodoService(UsersContext context)
    {
        _context = context;
    }
    
    public async Task<db.Todo> CreateTodo(db.Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
    
        return todo;
    }
}