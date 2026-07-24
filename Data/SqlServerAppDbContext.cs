using Microsoft.EntityFrameworkCore;

namespace StravaTeamApp.Data;

public sealed class SqlServerAppDbContext : AppDbContext
{
    public SqlServerAppDbContext(
        DbContextOptions<SqlServerAppDbContext> options)
        : base(options)
    {
    }
}