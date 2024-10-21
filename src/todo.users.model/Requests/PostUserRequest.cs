using System.Runtime.Serialization;

namespace todo.users.model.Requests;

[DataContract]
public class PostUserRequest
{
    [DataMember]
    public bool UseQueue;
    
    [DataMember(IsRequired = true)]
    public string Id { get; set; }
    
    [DataMember(IsRequired = true)]
    public string Username { get; set; }
    
    [DataMember(IsRequired = true)]
    public string FirstName { get; set; }

    [DataMember(IsRequired = true)]
    public string FamilyName { get; set; }
    
    [DataMember(IsRequired = true)]
    public string Email { get; set; }
}