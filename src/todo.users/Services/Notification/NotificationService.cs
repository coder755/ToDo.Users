using System.Collections.Concurrent;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using todo.users.model.Notification;
using todo.users.Services.SocketService;

namespace todo.users.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<string, IConnection> ConnectedClients = new ();
    private readonly SnsData _snsData;
    private readonly ILogger<NotificationService> _logger;
    
    public NotificationService(IServiceProvider serviceProvider, ILogger<NotificationService> logger, SnsData snsData)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _snsData = snsData;
    }

    public async Task ConfirmSubscription(SnsMessage message)
    {
        var snsClient = new AmazonSimpleNotificationServiceClient();

        var confirmRequest = new ConfirmSubscriptionRequest
        {
            Token = message.Token,
            TopicArn = _snsData.TopicArn
        };

        var response = await snsClient.ConfirmSubscriptionAsync(confirmRequest);
        
        _logger.LogInformation($"Subscription confirmation response: {response.HttpStatusCode}");
    }

    private static void AddClient(string clientId, IConnection socket)
    {
        ConnectedClients.TryAdd(clientId, socket);
    }

    private static void RemoveClient(string clientId)
    {
        ConnectedClients.TryRemove(clientId, out _);
    }
    
    public async Task StartWebSocket(HttpContext context)
    {
        var connection = _serviceProvider.GetRequiredService<Connection>();
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        connection.SetWebSocket(webSocket);
        var clientId = Guid.NewGuid().ToString();
        AddClient(clientId, connection);
        await connection.KeepReceiving();
        RemoveClient(clientId);
        await connection.Close();
    }
    
    public async Task BroadcastMessage(SnsMessage message)
    {
        var tasks = new List<Task>();

        foreach (var client in ConnectedClients.Values)
        {
            tasks.Add(client.HandleSnsMessage(message));
        }

        await Task.WhenAll(tasks);  // Wait until all messages are sent
    }
}