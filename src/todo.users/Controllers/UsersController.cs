using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using todo.users.model;
using todo.users.model.Requests;
using todo.users.model.Responses;
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
    public async Task<ActionResult<PostUserResponse>> PostUser([FromBody] PostUserRequest req)
    {
        var isValidGuid = Guid.TryParse(req.Id, out var userGuid);
        if (!isValidGuid)
        {
            return new BadRequestResult();
        }
        try
        {
            var user = new db.User()
            {
                ExternalId = userGuid,
                ThirdPartyId = userGuid,
                UserName = req.Username, 
                FirstName = req.FirstName,
                FamilyName = req.FamilyName,
                Email = req.Email,
                CreatedDate = DateTime.Now
            };
            await _userService.CreateUser(user);
            var storedUser = await _userService.FindUser(user.ExternalId);
            
            return new PostUserResponse
            {
                User = storedUser.ToModelObject()
            };
        }
        catch (SystemException e)
        {
            _logger.LogError(e.Message);
            return new BadRequestResult();
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<User>> Get()
    {
        var userId = _authHeaderProvider.GetUserId();
        var user = await _userService.FindUser(userId);
        
        if (user == null || user.ExternalId == Guid.Empty)
        {
            return new NoContentResult();
        }
        
        return  user.ToModelObject();
    }

    /// <summary>
    /// Will need to go through and manually delete the user from AWS Cognito console
    /// </summary>
    /// <param name="userToDeleteId"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{userToDeleteId}")]
    public async Task<ActionResult> HardDelete([FromRoute] string userToDeleteId)
    {
        var isValidGuid = Guid.TryParse(userToDeleteId, out var userGuid);
        if (!isValidGuid)
        {
            return new BadRequestResult();
        }

        try
        {
            var deleteUserSuccess = await _userService.DeleteUser(userGuid);
            return deleteUserSuccess ? new OkResult() : new StatusCodeResult(500);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}