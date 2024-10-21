using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using todo.users.model.Notification;
using todo.users.Services.Notification;

namespace todo.users.Controllers;

[ApiController]
[Route("api/[controller]/v1")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;


    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> StartNotificationWebsocket()
    {
        var context = ControllerContext.HttpContext;

        if (!context.WebSockets.IsWebSocketRequest) return new BadRequestResult();
        await _notificationService.StartWebSocket(context);
        return new EmptyResult();
    }
    
    [HttpPost("sns-listener")]
    public async Task<IActionResult> ReceiveSnsMessage([FromBody] SnsMessage snsMessage)
    {
        _logger.LogInformation($"Reached sns-listener: {snsMessage}");
        if (snsMessage.Type == "SubscriptionConfirmation")
        {
            await _notificationService.ConfirmSubscription(snsMessage);
        }

        await _notificationService.BroadcastMessage(snsMessage);
        return Ok();
    }
}