using JackLimited.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JackLimited.Infrastructure;

public class JackLimitedDbContext : DbContext
{
    public JackLimitedDbContext(DbContextOptions<JackLimitedDbContext> options) : base(options) { }

    public DbSet<Survey> Surveys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LikelihoodToRecommend).IsRequired();
            entity.Property(e => e.Comments).HasMaxLength(1000);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}

public class JackLimitedDbContextFactory : IDesignTimeDbContextFactory<JackLimitedDbContext>
{
    public JackLimitedDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JackLimitedDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=jacklimited;Username=postgres;Password=postgres");

        return new JackLimitedDbContext(optionsBuilder.Options);
    }
}