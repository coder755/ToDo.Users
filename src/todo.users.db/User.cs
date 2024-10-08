using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace todo.users.db;

[Index(nameof(ExternalId))]
[Index(nameof(ThirdPartyId), IsUnique = true)]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    [Required]
    public Guid ExternalId { get; set; }
    
    [Required]
    public Guid ThirdPartyId { get; set; }
    
    [Required]
    public string UserName { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string FamilyName { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }
}