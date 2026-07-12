using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StravaTeamApp.Models;

namespace StravaTeamApp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Club> Clubs { get; set; }
    public DbSet<Athlete> Athletes { get; set; }
    public DbSet<StravaActivity> Activities { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Club>().HasKey(t => t.Id);
        modelBuilder.Entity<Club>().Property(t => t.Id).ValueGeneratedNever();

        modelBuilder.Entity<Athlete>().HasKey(a => a.Id);
        modelBuilder.Entity<Athlete>().Property(a => a.Id).ValueGeneratedNever();

        modelBuilder.Entity<StravaActivity>().HasKey(a => a.Id);
        modelBuilder.Entity<StravaActivity>().Property(a => a.Id).ValueGeneratedNever();
    }
}