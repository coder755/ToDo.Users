using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using todo.users.model;

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
            if (response.IsSuccessStatusCode)
            {
                var user = JsonConvert.DeserializeObject<db.User>(response.Content.ReadAsStringAsync().Result);
                return user.ToModelObject();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        return new User();
    }
}