using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using todo.users.model;
using todo.users.model.Requests;
using todo.users.Services.Auth;
using todo.users.Services.Todo;
using todo.users.Services.User;

namespace todo.users.Controllers;

[ApiController]
[Route("api/[controller]/v1")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[Authorize]
public class TodoController
{
    private readonly ILogger<TodoController> _logger;
    private readonly ITodoService _todoService;
    private readonly IAuthHeaderProvider _authHeaderProvider;
    private readonly IUserService _userService;

    public TodoController(
        ILogger<TodoController> logger, 
        ITodoService todoService, 
        IAuthHeaderProvider authHeaderProvider, 
        IUserService userService)
    {
        _logger = logger;
        _todoService = todoService;
        _authHeaderProvider = authHeaderProvider;
        _userService = userService;
    }

    [Authorize]
    [HttpGet]
    public ActionResult<List<Todo>> GetAll()
    {
        var ownerId = _authHeaderProvider.GetUserId();
        var todos = _todoService.FindAllTodos(ownerId);
        
        return todos.Select(todo => todo.ToModelObject()).ToList();
    }
    
    [HttpPost]
    public async Task<ActionResult> PostTodo([FromBody] PostTodRequest req)
    {
        var userId = _authHeaderProvider.GetUserId();
        var user = await _userService.FindUser(userId);
        
        if (user == null || user.ExternalId == Guid.Empty)
        {
            return new BadRequestResult();
        }
        
        var todo = new db.Todo()
        {
            ExternalId = Guid.NewGuid(),
            UserId = userId,
            Name = req.Name, 
            IsComplete = false,
            CompleteDate = DateTime.Now,
            CreatedDate = DateTime.Now
        };
        try
        {
            await _todoService.CreateTodo(todo);
            return new OkResult();
        }
        catch (SystemException e)
        {
            _logger.LogError(e.Message);
            return new BadRequestResult();
        }
    }
    
    [HttpPost("{todoId}/completed")]
    public async Task<ActionResult> PostTodoCompleted([FromRoute] string todoId)
    {
        var userId = _authHeaderProvider.GetUserId();
        var user = await _userService.FindUser(userId);
        
        if (user == null || user.ExternalId == Guid.Empty)
        {
            return new BadRequestResult();
        }
        
        var isValidGuid = Guid.TryParse(todoId, out var todoExternalId);
        if (!isValidGuid)
        {
            return new BadRequestResult();
        }
 
        try
        {
            await _todoService.MarkTodoCompleted(todoExternalId, userId);
            return new OkResult();
        }
        catch (SystemException e)
        {
            _logger.LogError(e.Message);
            return new BadRequestResult();
        }
    }
}