namespace todo.users.model.Auth;

public class CognitoSettings
{
    public string UserPoolId { get; set; }
    public string Authority { get; set; }
    public string AppClientId { get; set; }
}