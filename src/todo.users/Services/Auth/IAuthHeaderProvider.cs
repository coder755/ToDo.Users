namespace todo.users.Services.Auth;

public interface IAuthHeaderProvider
{
    Guid GetUserId();

    string GetAuthHeaderValue();
}