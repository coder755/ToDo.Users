using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using todo.users.model;
using todo.users.model.Requests;
using todo.users.Services.Auth;
using todo.users.Services.User;

namespace todo.users.Controllers;

[ApiController]
[Route("api/[controller]/v1")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[Authorize]
public class UserController
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly IAuthHeaderProvider _authHeaderProvider;

    public UserController(
        IUserService userService,
        ILogger<UserController> logger,
        IAuthHeaderProvider authHeaderProvider
        )
    {
        _logger = logger;
        _authHeaderProvider = authHeaderProvider;
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult> RequestPostUser([FromBody] PostUserRequest req)
    {
        var isValidGuid = Guid.TryParse(req.Id, out var userGuid);
        if (!isValidGuid)
        {
            return new BadRequestResult();
        }
        try
        {
            var user = new User
            {
                ExternalId = userGuid,
                Username = req.Username,
                FirstName = req.FirstName,
                FamilyName = req.FamilyName,
                Email = req.Email,
            };
            var requestSubmitted = await _userService.RequestCreateUser(user);

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
    
    [HttpGet]
    public async Task<ActionResult<User>> Get()
    {
        var userId = _authHeaderProvider.GetUserId();
        var user = await _userService.FindUser(userId);
        
        if (user.IsEmptyUser())
        {
            return new NoContentResult();
        }
        
        return  user;
    }
}