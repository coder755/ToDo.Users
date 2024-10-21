using System.Runtime.Serialization;

namespace todo.users.model.Notification;

[DataContract]
public class SnsMessage
{
    [DataMember(IsRequired = false)]
    public string Type { get; set; }
    [DataMember(IsRequired = false)]
    public string MessageId { get; set; }
    [DataMember(IsRequired = false)]
    public string Token { get; set; }
    [DataMember(IsRequired = false)]
    public string TopicArn { get; set; }
    [DataMember(IsRequired = false)]
    public string Message { get; set; }
    [DataMember(IsRequired = false)]
    public string SubscribeURL { get; set; }
}