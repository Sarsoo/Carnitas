using Carnitas.Model.Governance;
using Carnitas.Model.Governance.Policy;
using Carnitas.Model.Identity;
using Carnitas.Model.Operations;
using Carnitas.Model.Source;
using Carnitas.Model.Source.SourceControl;
using Carnitas.Model.Source.SourceControl.GitHub;
using Carnitas.Model.Util;
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
    public DbSet<Module> Modules { get; set; }
    public DbSet<RootModule> RootModules { get; set; }
    public DbSet<Checkout> Checkouts { get; set; }
    public DbSet<OperationRun> OperationRuns { get; set; }
    public DbSet<OperationRunLogEntry> OperationRunLogEntries { get; set; }

    public DbSet<GitHubApp> GitHubApps { get; set; }

    public DbSet<Function> Functions { get; set; }
    public DbSet<PrincipalResourceFunction> PrincipalResourceFunctions { get; set; }
    
    public DbSet<InstanceSetting> InstanceSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.BuildOrganisation()
            .BuildRepository()
            .BuildCheckout();

        builder.BuildFunction()
            .BuildPrincipalResourceFunction();

        builder.BuildModule()
            .BuildOperationRun()
            .BuildOperationLogs();
    }
}

internal static class ModelBuilderExtensions
{
    extension(ModelBuilder builder)
    {
        public ModelBuilder BuildInstanceSettings()
        {
            builder.Entity<InstanceSetting>()
                .HasKey(e => e.Key);

            return builder;
        }

        public ModelBuilder BuildModule()
        {
            builder.Entity<Module>()
                .HasMany(e => e.OperationRuns)
                .WithOne(e => e.Module)
                .HasForeignKey(e => e.ModuleId)
                .HasPrincipalKey(e => e.Id);

            builder.Entity<Module>()
                .HasKey(e => e.Id);

            builder.Entity<RootModule>().ToTable("RootModule");

            return builder;
        }
        
        public ModelBuilder BuildOperationRun()
        {
            builder.Entity<OperationRun>()
                .ToTable("OperationRuns")
                .HasKey(e => e.Id);

            builder.Entity<OperationRun>()
                .HasOne(e => e.Module)
                .WithMany(e => e.OperationRuns)
                .HasForeignKey(e => e.ModuleId)
                .HasPrincipalKey(e => e.Id);

            builder.Entity<OperationRun>()
                .HasOne(e => e.Checkout)
                .WithMany()
                .HasForeignKey(e => e.CheckoutId)
                .HasPrincipalKey(e => e.Id)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<InitRun>().ToTable("InitRuns");
            builder.Entity<ApplyRun>().ToTable("ApplyRuns");
            builder.Entity<PlanRun>().ToTable("PlanRuns");

            return builder;
        }
        
        public ModelBuilder BuildOperationLogs()
        {
            builder.Entity<OperationRunLogEntry>()
                .ToTable("OperationRunLogEntries")
                .HasKey(e => e.Id);

            builder.Entity<OperationRunLogEntry>()
                .HasOne(e => e.OperationRun)
                .WithMany()
                .HasForeignKey(e => e.OperationRunId)
                .HasPrincipalKey(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OperationRunLogEntry>()
                .Property(e => e.Payload)
                .HasColumnType("jsonb");

            builder.Entity<OperationRunLogEntry>()
                .HasIndex(e => e.OperationRunId);
            builder.Entity<OperationRunLogEntry>()
                .HasIndex(e => new { e.OperationRunId, e.Sequence });
            builder.Entity<OperationRunLogEntry>()
                .HasIndex(e => e.Timestamp);
            builder.Entity<OperationRunLogEntry>()
                .HasIndex(e => e.Level);
            builder.Entity<OperationRunLogEntry>()
                .HasIndex(e => e.Type);

            return builder;
        }
    }

    #region Structure
    extension(ModelBuilder builder)
    {
        public ModelBuilder BuildOrganisation()
        {
            builder.Entity<Organisation>()
                .HasKey(e => e.Id);
            builder.Entity<Organisation>()
                .HasMany(e => e.Repositories)
                .WithOne(e => e.Organisation)
                .HasForeignKey(e => e.OrganisationId)
                .HasPrincipalKey(e => e.Id);

            return builder;
        }
        
        public ModelBuilder BuildRepository()
        {
            builder.Entity<Repository>()
                .HasKey(e => e.Id);
            
            builder.Entity<Repository>()
                .HasMany(e => e.Modules)
                .WithOne(e => e.Repository)
                .HasForeignKey(e => e.RepositoryId)
                .HasPrincipalKey(e => e.Id);

            builder.Entity<Repository>()
                .HasOne(e => e.GitHubApp)
                .WithMany()
                .HasForeignKey(e => e.GitHubAppId)
                .HasPrincipalKey(e => e.Id);
            
            return builder;
        }
        
        public ModelBuilder BuildCheckout()
        {
            builder.Entity<Checkout>()
                .HasKey(e => e.Id);
            builder.Entity<Checkout>()
                .HasOne(e => e.Repository)
                .WithMany(e => e.Checkouts)
                .HasForeignKey(e => e.RepositoryId)
                .HasPrincipalKey(e => e.Id);

            return builder;
        }
    }
    #endregion
    
    #region Governance
    extension(ModelBuilder builder)
    {
        public ModelBuilder BuildFunction()
        {
            builder.Entity<Function>()
                .HasKey(e => e.Name);

            builder.Entity<Function>()
                .HasMany(e => e.PrincipalResourceFunctions)
                .WithOne(e => e.Function)
                .HasForeignKey(e => e.FunctionName)
                .HasPrincipalKey(e => e.Name);

            return builder;
        }

        public ModelBuilder BuildPrincipalResourceFunction()
        {
            builder.Entity<PrincipalResourceFunction>()
                .HasKey(e => new {e.PrincipalId, e.PrincipalType, e.ResourceId, e.ResourceType, e.FunctionName});
            builder.Entity<PrincipalResourceFunction>()
                .HasOne<Function>(e => e.Function);

            return builder;
        }
    }
    #endregion
}