using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StravaTeamApp.Models;
using StravaTeamApp.Models.Badges;

namespace StravaTeamApp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Club> Clubs { get; set; }
    public DbSet<Athlete> Athletes { get; set; }
    public DbSet<StravaActivity> Activities { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }

    public DbSet<Badge> Badges { get; set; }
    public DbSet<BadgeRule> BadgeRules { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var targetValueCheckConstraint =
        Database.IsSqlServer() ? "[TargetValue] > 0" : "\"TargetValue\" > 0";

        modelBuilder.Entity<Club>().HasKey(t => t.Id);
        modelBuilder.Entity<Club>()
            .Property(t => t.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Athlete>().HasKey(a => a.Id);
        modelBuilder.Entity<Athlete>()
            .Property(a => a.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<StravaActivity>().HasKey(a => a.Id);
        modelBuilder.Entity<StravaActivity>()
            .Property(a => a.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<SystemLog>(systemLog =>
        {
            systemLog.HasKey(log => log.Id);

            systemLog.Property(log => log.Category)
                .HasDefaultValue(
                    SystemLogCategories.System);

            systemLog.Property(log => log.EventName)
                .HasDefaultValue("LegacyEvent");

            systemLog.HasIndex(log => log.CreatedAtUtc);
            systemLog.HasIndex(log => log.Level);
            systemLog.HasIndex(log => log.Category);
        });

        modelBuilder.Entity<Badge>(badge =>
        {
            badge.HasKey(b => b.Id);

            badge.HasIndex(b => b.IsActive);

            badge.HasOne(b => b.Rule)
                .WithOne(r => r.Badge)
                .HasForeignKey<BadgeRule>(r => r.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);

            badge.HasMany(b => b.Awards)
                .WithOne(ub => ub.Badge)
                .HasForeignKey(ub => ub.BadgeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BadgeRule>(rule =>
        {
            rule.HasKey(r => r.BadgeId);

            rule.Property(r => r.Metric)
                .HasConversion<int>();

            rule.Property(r => r.Operation)
                .HasConversion<int>();

            rule.Property(r => r.PeriodType)
                .HasConversion<int>();

            rule.ToTable(
                "BadgeRules",
                table => table.HasCheckConstraint(
                    "CK_BadgeRules_TargetValue_Positive",
                    targetValueCheckConstraint));
        });

        modelBuilder.Entity<UserBadge>(userBadge =>
        {
            userBadge.HasKey(ub => ub.Id);

            userBadge.HasOne(ub => ub.User)
                .WithMany()
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            userBadge.HasOne(ub => ub.TriggerActivity)
                .WithMany()
                .HasForeignKey(ub => ub.TriggerActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            userBadge.HasIndex(ub => new
            {
                ub.UserId,
                ub.BadgeId,
                ub.PeriodStartUtc,
                ub.PeriodEndUtc
            })
                .IsUnique();

            userBadge.HasIndex(ub => ub.AwardedAtUtc);
        });
    }
}
