using System.Net;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using todo.users.model.Queue;

namespace todo.users.Services.Queue;

public class QueueService : IQueueService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly SqsData _sqsData;
    private readonly ILogger<QueueService> _logger;
    private const string Typekey = "Type";
    private const string StringDataType = "String";

    public QueueService(IAmazonSQS sqsClient, SqsData sqsData, ILogger<QueueService> logger)
    {
        _sqsClient = sqsClient;
        _sqsData = sqsData;
        _logger = logger;
    }

    public async Task<bool> AddCreateUserReqToQueue(model.User user)
    {
        var messageBody = JsonConvert.SerializeObject(user);
        var attributes = new Dictionary<string, MessageAttributeValue>
        {
            {
                Typekey, new MessageAttributeValue
                {
                    DataType = StringDataType,
                    StringValue = MessageTypes.CreateUser.ToString()
                }
            }
        };
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = _sqsData.ToProcessQueueUrl,
            MessageBody = messageBody,
            MessageAttributes = attributes
        };

        return await TryToSendMessage(sendMessageRequest);
    }

    public async Task<bool> AddCreateTodoReqToQueue(Guid userId, model.Todo todo)
    {
        var createTodoQueueMessage = new CreateTodoQueueMessage
        {
            UserId = userId,
            Todo = todo
        };
        var messageBody = JsonConvert.SerializeObject(createTodoQueueMessage);
        var attributes = new Dictionary<string, MessageAttributeValue>
        {
            {
                Typekey, new MessageAttributeValue
                {
                    DataType = StringDataType,
                    StringValue = MessageTypes.CreateTodo.ToString()
                }
            }
        };
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = _sqsData.ToProcessQueueUrl,
            MessageBody = messageBody,
            MessageAttributes = attributes
        };

        return await TryToSendMessage(sendMessageRequest);
    }
    
    private async Task<bool> TryToSendMessage(SendMessageRequest request)
    {
        try
        {
            var response = await _sqsClient.SendMessageAsync(request);
            if (response.HttpStatusCode.Equals(HttpStatusCode.OK))
            {
                _logger.LogInformation($"Message sent with ID: {response.MessageId}");
                return true;
            }
            _logger.LogError($"Error sending message with ID: {response.MessageId}");
        }
        catch (Exception e)
        {
            _logger.LogError($"Error sending message: {e.Message}");
        }
        return false;
    }
}