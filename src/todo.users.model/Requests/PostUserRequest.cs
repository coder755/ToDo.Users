using System.Runtime.Serialization;

namespace todo.users.model.Requests;

[DataContract]
public class PostUserRequest
{
    [DataMember(IsRequired = true)]
    public string Id { get; set; }
    
    /// <summary>
    /// Username of user
    /// </summary>
    [DataMember(IsRequired = true)]
    public string Username { get; set; }
    
    /// <summary>
    /// The user's first name
    /// </summary>
    [DataMember(IsRequired = true)]
    public string FirstName { get; set; }

    /// <summary>
    /// The user's family name
    /// </summary>
    [DataMember(IsRequired = true)]
    public string FamilyName { get; set; }
    
    /// <summary>
    /// The user's email
    /// </summary>
    [DataMember(IsRequired = true)]
    public string Email { get; set; }
}