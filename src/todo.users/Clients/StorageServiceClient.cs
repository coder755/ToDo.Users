using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using todo.users.model;
using todo.users.model.Requests;

namespace todo.users.Clients;

public class StorageServiceClient : IStorageServiceClient
{
    private readonly ILogger<StorageServiceClient> _logger;
    private static HttpClient _client;

    public StorageServiceClient(ILogger<StorageServiceClient> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }
    
    public async Task<bool> RequestCreateUser(User user)
    {
        var userJson = JsonConvert.SerializeObject(user);
        var content = new StringContent(userJson, Encoding.UTF8, MediaTypeNames.Application.Json);
        var uri = new Uri("api/user/v1/", UriKind.Relative);

        try
        {
            var response = await _client.PostAsync(uri, content);
            if (response.StatusCode.Equals(HttpStatusCode.Accepted))
            {
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        return false;
    }

    public async Task<User> GetUser(Guid userId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("api/user/v1/" + userId, UriKind.Relative)
        };
        
        try
        {
            var response = await _client.SendAsync(request);
            if (response.StatusCode.Equals(HttpStatusCode.NoContent))
            {
                return new User();
            }
            if (response.IsSuccessStatusCode)
            {
                var user = JsonConvert.DeserializeObject<User>(response.Content.ReadAsStringAsync().Result);
                return user;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        return new User();
    }

    public async Task<List<Todo>> GetAllTodos(Guid userId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("api/todo/v1/" + userId, UriKind.Relative)
        };
        
        try
        {
            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var todos = JsonConvert.DeserializeObject<List<Todo>>(response.Content.ReadAsStringAsync().Result);
                return todos;
            }

            throw new Exception("Received failed status code from storage service");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
    
    public async Task<bool> RequestCreateTodo(Guid userId, Todo todo)
    {
        var todoJson = JsonConvert.SerializeObject(todo);
        var content = new StringContent(todoJson, Encoding.UTF8, MediaTypeNames.Application.Json);
        var uri = new Uri($"api/todo/v1/{userId.ToString()}", UriKind.Relative);

        try
        {
            var response = await _client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        return false;
    }
    public async Task<bool> RequestMarkTodoCompleted(Guid userId, Guid todoId)
    {
        var reqJson = JsonConvert.SerializeObject(new PostTodoCompletedRequest
        {
            TodoId = todoId
        });
        var content = new StringContent(reqJson, Encoding.UTF8, MediaTypeNames.Application.Json);

        var uri = new Uri($"api/todo/v1/{userId.ToString()}/completed", UriKind.Relative);

        try
        {
            var response = await _client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        return false;
    }
}