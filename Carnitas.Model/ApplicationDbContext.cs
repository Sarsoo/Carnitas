using Carnitas.Model.Governance;
using Carnitas.Model.Governance.Policy;
using Carnitas.Model.Identity;
using Carnitas.Model.Operations;
using Carnitas.Model.Source;
using Carnitas.Model.Source.GitHub;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Carnitas.Model;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSeeding((o, _) =>
        {
            var admin = o.Set<IdentityRole>().SingleOrDefault(r => r.Name == "Admin");
            if (admin is null)
            {
                o.Set<IdentityRole>().Add(new IdentityRole("Admin"));
                o.SaveChanges();
            }
        })
        .UseAsyncSeeding(async (o, _, ct) =>
        {
            var admin = await o.Set<IdentityRole>().SingleOrDefaultAsync(r => r.Name == "Admin", cancellationToken: ct);
            if (admin is null)
            {
                o.Set<IdentityRole>().Add(new IdentityRole("Admin"));
                await o.SaveChangesAsync(ct);
            }
        });
    }

    public DbSet<Organisation> Organisations { get; set; }
    public DbSet<Repository> Repository { get; set; }
    public DbSet<RootModule> RootModules { get; set; }

    public DbSet<GitHubApp> GitHubApps { get; set; }

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

        builder.Entity<Repository>()
            .HasOne(e => e.GitHubApp)
            .WithMany()
            .HasForeignKey(e => e.GitHubAppId)
            .HasPrincipalKey(e => e.Id);

        builder.Entity<Function>()
            .HasKey(e => e.Name);

        builder.Entity<Function>()
            .HasMany(e => e.PrincipalResourceFunctions)
            .WithOne(e => e.Function)
            .HasForeignKey(e => e.FunctionName)
            .HasPrincipalKey(e => e.Name);

        builder.Entity<PrincipalResourceFunction>()
            .HasKey(e => new {e.PrincipalId, e.PrincipalType, e.ResourceId, e.ResourceType, e.FunctionName});
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