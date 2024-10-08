using Microsoft.EntityFrameworkCore;

namespace todo.users.db;

public class UsersContext : DbContext
{
    // public UsersContext(DbContextOptions<UsersContext> options, DbSet<User> users) : base(options)
    public UsersContext(DbContextOptions<UsersContext> options) : base(options)
    {
        ChangeTracker.LazyLoadingEnabled = false;
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Todo> Todos { get; set; }
}