#nullable enable
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using todo.users.model.Exceptions;
using todo.users.model.Notification;
using todo.users.model.Socket;
using todo.users.Services.Auth.User;

namespace todo.users.Services.SocketService;

public class Connection : IConnection
{
    private readonly ILogger<Connection> _logger;
    private readonly IAuthUserProvider _authUserProvider;
    private WebSocket? _webSocket;
    private Guid _userId = Guid.Empty;

    public Connection(ILogger<Connection> logger, IAuthUserProvider authUserProvider)
    {
        _logger = logger;
        _authUserProvider = authUserProvider;
    }

    public void SetWebSocket(WebSocket? webSocket)
    {
        _webSocket = webSocket;
        _logger.LogInformation("Websocket Initiated!!");
    }

    public WebSocket? GetWebSocket()
    {
        return _webSocket;
    }
    
    public async Task<WebSocketCloseStatus?> KeepReceiving()
    {
        WebSocketReceiveResult message;
        do
        {
            using var memoryStream = new MemoryStream();
            message = await ReceiveMessage(memoryStream);
            if (message.Count <= 0) continue;
            try
            {
                var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                HandleSocketMessage(receivedMessage);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        } while (message.MessageType != WebSocketMessageType.Close);

        return message.CloseStatus;
    }
    
    private async Task<WebSocketReceiveResult> ReceiveMessage(MemoryStream memoryStream)
    {
        var readBuffer = new ArraySegment<byte>(new byte[4 * 1024]);
        WebSocketReceiveResult result;
        do
        {
            if (_webSocket is null) throw new Exception("Websocket not initialized");
            result = await _webSocket.ReceiveAsync(readBuffer, CancellationToken.None);
            await memoryStream.WriteAsync(readBuffer.Array.AsMemory(readBuffer.Offset, result.Count),
                CancellationToken.None);
        } while (!result.EndOfMessage);

        return result;
    }
    
    public Task HandleSnsMessage(SnsMessage message)
    {
        if (_webSocket is null) return Task.CompletedTask;
        if (_webSocket?.State == WebSocketState.Open)
        {
            HandleSocketMessage(message.Message);
        }

        return Task.CompletedTask;
    }
    
    private void HandleSocketMessage(string receivedMessage)
    {
        try
        {
            var messageRequest = JsonConvert.DeserializeObject<SocketMessageRequest>(receivedMessage);
            if (messageRequest == null) throw new Exception();
            switch (messageRequest.Type)
            {
                case IncomingMessageType.AddTokenRequest:
                {
                    var addTokenRequest = JsonConvert.DeserializeObject<AddTokenRequest>(receivedMessage);
                    if (addTokenRequest == null) throw new Exception();
                    HandleAddTokensRequest(addTokenRequest);
                    break;
                }
                case IncomingMessageType.UserCreated:
                {
                    var userCreatedMessage = JsonConvert.DeserializeObject<UserCreatedMessage>(receivedMessage);
                    if (userCreatedMessage == null) throw new Exception();
                    HandleUserCreatedMessage(userCreatedMessage);
                    break;
                }
                case IncomingMessageType.TodoCreated:
                {
                    var todoCreatedMessage = JsonConvert.DeserializeObject<TodoCreatedMessage>(receivedMessage);
                    if (todoCreatedMessage == null) throw new Exception();
                    HandleTodoCreatedMessage(todoCreatedMessage);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Missing handling for MessageType: {messageRequest.Type}");
            }
        }
        catch (BadActorException)
        {
            _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
        catch(Exception e)
        {
            _logger.LogError(e.Message);
            _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }

    private async void HandleAddTokensRequest(AddTokenRequest addTokenRequest)
    {
        try
        {
            await _authUserProvider.ValidateTokenAsync(addTokenRequest.Token); 
            _userId = _authUserProvider.GetUserId(addTokenRequest.Token);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw new BadActorException();
        }
    }

    private async void HandleUserCreatedMessage(UserCreatedMessage userCreatedMessage)
    {
        if (userCreatedMessage.UserId.Equals(_userId))
        {
            await Send(IncomingMessageType.UserCreated.ToString());
        }
    }

    private async void HandleTodoCreatedMessage(TodoCreatedMessage todoCreatedMessage)
    {
        if (todoCreatedMessage.UserId.Equals(_userId))
        {
            await Send(IncomingMessageType.TodoCreated.ToString());
        }
    }
    
    public async Task Send(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (_webSocket is null) throw new Exception("Websocket not initialized");
        await _webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }

    public async Task Close()
    {
        if (_webSocket is null) throw new Exception("Websocket not initialized");
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }
}