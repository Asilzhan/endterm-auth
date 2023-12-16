using EndtermAuth.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EndtermAuth.Data;

public class AppDbContext : DbContext
{
    public required DbSet<User> Users { get; set; }
    public required DbSet<UserLogin> UserLogins { get; set; }
    public required DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=app.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserLoginConfiguration());
    }
}