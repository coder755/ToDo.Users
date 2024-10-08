using System.Runtime.Serialization;

namespace todo.users.model.Requests;

[DataContract]
public class PostTodRequest
{
    [DataMember(IsRequired = true)]
    public string Name { get; set; }
}