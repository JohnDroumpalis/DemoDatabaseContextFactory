using Microsoft.EntityFrameworkCore;

namespace WebApplication1;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    
    public DbSet<User> Users => Set<User>();

    public DatabaseContext() { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();
        });
    }
    
}
