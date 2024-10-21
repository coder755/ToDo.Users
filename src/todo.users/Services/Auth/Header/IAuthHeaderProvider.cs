namespace todo.users.Services.Auth.Header;

public interface IAuthHeaderProvider
{
    Guid GetUserId();
}