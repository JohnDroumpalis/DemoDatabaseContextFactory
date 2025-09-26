using Microsoft.EntityFrameworkCore;

namespace WebApplication1;

public class UserRepository
{
    private readonly IDbContextFactory<DatabaseContext> _contextFactory;

    public UserRepository(IDbContextFactory<DatabaseContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

   public async Task<RepoResult<User>> AddUserAsync(string name)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = new User { Name = name };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return new RepoResult<User>(true, $"Added {user.Name}", user);
    }

    public async Task<RepoResult<User>> UpdateUserAsync(int id, string newName)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return new RepoResult<User>(false, $"User {id} not found", null);

        user.Name = newName;
        await context.SaveChangesAsync();
        return new RepoResult<User>(true, $"Updated user {id}", user);
    }

    public async Task<RepoResult<bool>> DeleteUserAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return new RepoResult<bool>(false, $"User {id} not found", false);

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return new RepoResult<bool>(true, $"Deleted user {id}", true);
    }

    public async Task<RepoResult<User?>> GetUserByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return new RepoResult<User?>(user != null, user != null ? $"Found user {id}" : $"User {id} not found", user);
    }

    public async Task<RepoResult<List<User>>> GetAllUsersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var users = await context.Users.AsNoTracking().ToListAsync();
        return new RepoResult<List<User>>(true, $"Fetched {users.Count} users", users);
    }
    
}
public record RepoResult<T>(bool Success, string Message, T? Data);