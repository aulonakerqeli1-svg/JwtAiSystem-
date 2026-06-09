using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> opts)
        : base(opts) { }

    public DbSet<User> Users { get; set; }
    public DbSet<LoginEvent> LoginEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        mb.Entity<LoginEvent>()
            .HasIndex(e => new { e.Username, e.Timestamp });
    }
}