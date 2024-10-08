using System.Net;
using todo.users.model;

namespace todo.users;

public static class Extensions
{
    public static User ToModelObject(this db.User dbUser)
    {
        return new User
        {
            Email = dbUser.Email,
            ExternalId = dbUser.ExternalId,
            FamilyName = dbUser.FamilyName,
            FirstName = dbUser.FirstName,
            Username = dbUser.UserName
        };
    }
    
    public static Todo ToModelObject(this db.Todo dbTodo)
    {
        return new Todo
        {
            ExternalId = dbTodo.ExternalId,
            Name = dbTodo.Name,
            IsComplete = dbTodo.IsComplete,
            CompleteDate = dbTodo.CompleteDate
        };
    }
}