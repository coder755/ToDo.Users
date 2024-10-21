using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace todo.users.model.Queue;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageTypes
{
    [EnumMember(Value = "CreateUser")]
    CreateUser,
}