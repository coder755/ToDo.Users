using Microsoft.EntityFrameworkCore;
using todo.users.db;

namespace todo.users.Services.User;

public class UserService : IUserService
{
    private readonly UsersContext _context;


    public UserService( UsersContext context)
    {
        _context = context;
    }
    
    public async Task<db.User> FindUser(Guid externalId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user =>
            user.ExternalId == externalId
        );
        return user;
    }

    public async Task<db.User> FindThirdPartyUser(Guid thirdPartyId)
    {
        var user = await _context.Users.FirstAsync(user =>
            user.ThirdPartyId == thirdPartyId
        );
        return user;
    }
    
    public async Task<db.User> CreateUser(db.User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    
        return user;
    }

    public async Task<bool> DeleteUser(Guid externalId)
    {
        var user = await FindUser(externalId);
        if (user == null || user.ExternalId == Guid.Empty)
        {
            throw new Exception("User does not exist");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}