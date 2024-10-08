using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace todo.users.db;

[Index(nameof(ExternalId))]
public class Todo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    [Required]
    public Guid ExternalId { get; set; }
    
    [ForeignKey("UserId")]
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public bool IsComplete { get; set; }
    
    [Required]
    public DateTime CompleteDate { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }
}