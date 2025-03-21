﻿using Microsoft.Extensions.Logging;
using todo.users.Clients;
using todo.users.Services.Queue;

namespace todo.users.Services.Todo;

public class TodoService : ITodoService
{
    private readonly ILogger<TodoService> _logger;
    private readonly IStorageServiceClient _storageServiceClient;
    private readonly IQueueService _queueService;
    public TodoService(ILogger<TodoService> logger, IStorageServiceClient storageServiceClient, IQueueService queueService)
    {
        _logger = logger;
        _storageServiceClient = storageServiceClient;
        _queueService = queueService;
    }
    
    public async Task<bool> RequestCreateTodo(Guid userId, model.Todo todo, bool useQueueService)
    {
        try
        {
            if (useQueueService)
            {
                var qSuccess = await _queueService.AddCreateTodoReqToQueue(userId, todo);
                return qSuccess;
            }
            var success = await _storageServiceClient.RequestCreateTodo(userId, todo);
            return success;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return false;
    }
    
    public async Task<List<model.Todo>> GetAllTodos(Guid userId)
    {
        try
        {
            var todos = await _storageServiceClient.GetAllTodos(userId);
            return todos;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task<bool> RequestMarkTodoCompleted(Guid userId, Guid todoExternalId)
    {
        try
        {
            var success = await _storageServiceClient.RequestMarkTodoCompleted(userId, todoExternalId);
            return success;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return false;
    }

}