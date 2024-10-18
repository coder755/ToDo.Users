using Microsoft.Extensions.Logging;
using todo.users.Clients;

namespace todo.users.Services.User;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IStorageServiceClient _storageServiceClient;
    
    public UserService(ILogger<UserService> logger, IStorageServiceClient storageServiceClient)
    {
        _logger = logger;
        _storageServiceClient = storageServiceClient;
    }

    public async Task<bool> RequestCreateUser(model.User user)
    {
        try
        {
            var success = await _storageServiceClient.RequestCreateUser(user);
            return success;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return false;
    }
    
    public async Task<model.User> FindUser(Guid externalId)
    {
        try
        {
            var user = await _storageServiceClient.GetUser(externalId);
            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return new model.User();
    }
}