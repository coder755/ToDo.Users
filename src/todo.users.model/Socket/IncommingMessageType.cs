using System.Runtime.Serialization;

namespace todo.users.model.Socket;

public enum IncomingMessageType
{
    [EnumMember(Value = "AddTokenRequest")]
    AddTokenRequest,
    [EnumMember(Value = "UserCreated")]
    UserCreated,
    [EnumMember(Value = "TodoCreated")]
    TodoCreated
}