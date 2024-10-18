using System.Runtime.Serialization;

namespace todo.users.model.Requests;

[DataContract]
public class PostTodoCompletedRequest
{
    [DataMember(IsRequired = true)]
    public Guid TodoId { get; set; }
}