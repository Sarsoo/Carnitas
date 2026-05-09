using Carnitas.Model.Governance;
using Carnitas.Model.Governance.Policy;
using Carnitas.Model.Identity;
using Carnitas.Model.Operations;
using Carnitas.Model.Source;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Carnitas.Model;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organisation> Organisations { get; set; }
    public DbSet<Repository> Repository { get; set; }
    public DbSet<RootModule> RootModules { get; set; }

    public DbSet<Function> Functions { get; set; }
    public DbSet<PrincipalResourceFunction> PrincipalResourceFunctions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Organisation>()
            .HasKey(e => e.Id);
        builder.Entity<Organisation>()
            .HasMany(e => e.Repositories)
            .WithOne(e => e.Organisation)
            .HasForeignKey(e => e.OrganisationId)
            .HasPrincipalKey(e => e.Id);

        builder.Entity<Repository>()
            .HasKey(e => e.Id);
        builder.Entity<Repository>()
            .HasMany(e => e.RootModules)
            .WithOne(e => e.Repository)
            .HasForeignKey(e => e.RepositoryId)
            .HasPrincipalKey(e => e.Id);

        builder.Entity<Function>()
            .HasKey(e => e.Name);

        builder.Entity<PrincipalResourceFunction>()
            .HasKey(e => new {e.PrincipalId, e.PrincipalType, e.ResourceId, e.ResourceType, e.Function});
        builder.Entity<PrincipalResourceFunction>()
            .HasOne<Function>(e => e.Function);

        builder.Entity<RootModule>()
            .HasKey(e => e.Id);

        builder.Entity<ApplyRun>()
            .ToTable("ApplyRuns")
            .HasKey(e => e.Id);

        builder.Entity<PlanRun>()
            .ToTable("PlanRuns")
            .HasKey(e => e.Id);
    }
}