using Microsoft.EntityFrameworkCore;
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
    
    private async Task<db.Todo> FindTodo(Guid externalId, Guid userId)
    {
        var todo = await _context.Todos.FirstOrDefaultAsync(td =>
            td.ExternalId == externalId && td.UserId == userId
        );
        return todo;
    }
    
    public IEnumerable<db.Todo> FindAllTodos(Guid userId)
    {
        var todos = _context.Todos.Where(todo => todo.UserId.Equals(userId)).ToList();
        return todos;
    }

    public async Task<db.Todo> MarkTodoCompleted(Guid todoExternalId, Guid userId)
    {
        var foundTodo = await FindTodo(todoExternalId, userId);
        if (foundTodo.ExternalId.Equals(Guid.Empty))
        {
            throw new Exception();
        }

        foundTodo.IsComplete = true;
        foundTodo.CompleteDate = DateTime.Now;

        _context.Todos.Update(foundTodo);
        await _context.SaveChangesAsync();
        return foundTodo;
    }

}