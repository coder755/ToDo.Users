namespace todo.users.Services.Auth.User;

public interface IAuthUserProvider
{
    Task ValidateTokenAsync(string token);
    Guid GetUserId(string token);
}