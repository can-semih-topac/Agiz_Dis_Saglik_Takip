using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<GoalStatus> GoalStatuses => Set<GoalStatus>();
    public DbSet<StatusNote> StatusNotes => Set<StatusNote>();
    public DbSet<Suggestion> Suggestions => Set<Suggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordEncrypted).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.Property(g => g.Title).HasMaxLength(150).IsRequired();
            entity.Property(g => g.Description).HasMaxLength(500).IsRequired();

            entity.HasOne(g => g.User)
                  .WithMany(u => u.Goals)
                  .HasForeignKey(g => g.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GoalStatus>(entity =>
        {
            entity.HasOne(gs => gs.Goal)
                  .WithMany(g => g.GoalStatuses)
                  .HasForeignKey(gs => gs.GoalId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StatusNote>(entity =>
        {
            entity.Property(sn => sn.Description).HasMaxLength(1000).IsRequired();
            entity.Property(sn => sn.ImagePath).HasMaxLength(500);

            entity.HasOne(sn => sn.User)
                  .WithMany(u => u.StatusNotes)
                  .HasForeignKey(sn => sn.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Suggestion>(entity =>
        {
            entity.Property(s => s.Text).HasMaxLength(500).IsRequired();
        });
    }
}
