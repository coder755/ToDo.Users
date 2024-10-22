using System.Runtime.Serialization;

namespace todo.users.model.Requests;

[DataContract]
public class PostTodoRequest
{
    [DataMember]
    public bool UseQueue;
    [DataMember(IsRequired = true)]
    public string Name { get; set; }
}