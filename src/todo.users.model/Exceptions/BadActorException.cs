namespace todo.users.model.Exceptions;

public class BadActorException : Exception
{
    public BadActorException()
    {
    }

    public BadActorException(string message)
        : base(message)
    {
    }

    public BadActorException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}