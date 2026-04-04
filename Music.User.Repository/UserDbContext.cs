using Microsoft.EntityFrameworkCore;
using Music.User.Repository.Model;

namespace Music.User.Repository;

public class UserDbContext(DbContextOptions<UserDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Users>().HasKey(s => s.Id);
        modelBuilder.Entity<Users>().ToTable("Users");
    }

    public DbSet<Users> UsersEnumerable { get; set; }
}