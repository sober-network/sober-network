using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.Entities;

namespace SoberNetwork.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Rename Identity tables to snake_case for Postgres convention
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.HasDefaultSchema("public");
    }
}
