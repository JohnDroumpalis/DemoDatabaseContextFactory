using Microsoft.EntityFrameworkCore;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<DatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/parallel", async (UserRepository repo, IDbContextFactory<DatabaseContext> factory) =>
{
    // Ensure some base users
    await using (var ctx = await factory.CreateDbContextAsync())
    {
        if (!ctx.Users.Any())
        {
            ctx.Users.AddRange(
                new User { Name = "Alice" },
                new User { Name = "Bob" },
                new User { Name = "Charlie" },
                new User { Name = "Diana" },
                new User { Name = "Eve" }
            );
            await ctx.SaveChangesAsync();
        }
    }

    var tasks = new List<Task>();

    // 50 adds
    for (var i = 0; i < 50; i++)
        tasks.Add(repo.AddUserAsync($"NewUser_{i}"));

    // 20 updates (ids 1–20)
    for (var i = 1; i <= 20; i++)
        tasks.Add(repo.UpdateUserAsync(i, $"Updated_User_{i}"));

    // 10 deletes (ids 10–20)
    for (var i = 10; i <= 20; i++)
        tasks.Add(repo.DeleteUserAsync(i));

    // 20 reads of single users
    for (var i = 1; i <= 20; i++)
        tasks.Add(repo.GetUserByIdAsync(i));

    // 10 reads of all users
    for (var i = 0; i < 10; i++)
        tasks.Add(repo.GetAllUsersAsync());

    await Task.WhenAll(tasks);
    
    var adds = tasks.OfType<Task<RepoResult<User>>>().Select(t => t.Result).ToList();
    var updates = tasks.OfType<Task<RepoResult<User>>>().Where(t => t.Result != null).Select(t => t.Result).ToList();
    var deletes = tasks.OfType<Task<RepoResult<bool>>>().Select(t => t.Result).ToList();
    var singleReads = tasks.OfType<Task<RepoResult<User?>>>().Select(t => t.Result).ToList();
    var snapshots = tasks.OfType<Task<RepoResult<List<User>>>>().Select(t => t.Result).ToList();

    return new
    {
        AddedCount = adds.Count,
        UpdatedCount = updates.Count(x => x.Success),
        DeletedCount = deletes.Count(x => x.Success),
        SingleReads = singleReads,
        SnapshotCounts = snapshots.Select(s => s.Data.Count)
    };
});

app.Run();
