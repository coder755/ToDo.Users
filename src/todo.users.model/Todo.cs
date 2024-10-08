namespace todo.users.model;

public class Todo
{
    public Guid ExternalId { get; set; }
    public string Name { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CompleteDate { get; set; }
    
    public Todo()
    {
        ExternalId = new Guid();
        Name = "";
        IsComplete = false;
        CompleteDate = DateTime.Now;
    }
}