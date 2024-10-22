using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using todo.users.model;
using todo.users.model.Requests;
using todo.users.Services.Auth.Header;
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
    public async Task<ActionResult<List<Todo>>> GetAll()
    {
        var ownerId = _authHeaderProvider.GetUserId();
        var todos = await _todoService.GetAllTodos(ownerId);
        
        return todos.ToList();
    }
    
    [HttpPost]
    public async Task<ActionResult> RequestPostTodo([FromBody] PostTodoRequest req)
    {
        var userId = _authHeaderProvider.GetUserId();
        var user = await _userService.FindUser(userId);
        
        if (user.IsEmptyUser())
        {
            return new BadRequestResult();
        }
        
        var todo = new Todo()
        {
            ExternalId = Guid.NewGuid(),
            Name = req.Name, 
            IsComplete = false,
            CompleteDate = DateTime.Now,
        };
        try
        {
            var requestSubmitted = await _todoService.RequestCreateTodo(userId, todo, req.UseQueue);
            if (requestSubmitted)
            {
                return new AcceptedResult();
            }
        }
        catch (SystemException e)
        {
            _logger.LogError(e.Message);
            return new BadRequestResult();
        }
        return new StatusCodeResult(500);
    }
    
    [HttpPost("{todoId}/completed")]
    public async Task<ActionResult> PostTodoCompleted([FromRoute] string todoId)
    {
        var userId = _authHeaderProvider.GetUserId();
        var user = await _userService.FindUser(userId);
        
        if (user.IsEmptyUser())
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
            var requestSubmitted = await _todoService.RequestMarkTodoCompleted(userId, todoExternalId);
            if (requestSubmitted)
            {
                return new AcceptedResult();
            }
        }
        catch (SystemException e)
        {
            _logger.LogError(e.Message);
            return new BadRequestResult();
        }
        return new StatusCodeResult(500);
    }
}