using Microsoft.EntityFrameworkCore;

namespace IntervalsMcp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ActivityEntity> Activities => Set<ActivityEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityEntity>(e =>
        {
            e.ToTable("activities");
            e.HasKey(a => a.Id);
            // Data "lokalna" z intervals.icu nie ma strefy czasowej
            e.Property(a => a.StartDateLocal).HasColumnType("timestamp without time zone");
            e.HasIndex(a => a.StartDateLocal);
        });
    }
}
