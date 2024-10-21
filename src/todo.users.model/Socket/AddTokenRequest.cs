namespace todo.users.model.Socket;

public class AddTokenRequest : SocketMessageRequest
{
    public string Token { get; set; }
}