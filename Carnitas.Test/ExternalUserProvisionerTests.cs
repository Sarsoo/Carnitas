using Carnitas.Model;
using Carnitas.Model.Identity;
using Carnitas.Web.Identity.Ldap;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Carnitas.Test;

public class ExternalUserProvisionerTests
{
    private const string UserPrefix = "ldap-test-";

    [Fact]
    public async Task UnknownDirectoryUserIsProvisionedConfirmedAndLinked()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareAsync(connectionString);
        using var host = new ProvisionerHost(connectionString, new LdapOptions { AutoProvision = true });

        var info = Info("guid-1", $"{UserPrefix}andy", "andy@corp.example.com");

        await host.Provisioner.EnsureProvisionedAsync(info);

        var user = await host.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        Assert.NotNull(user);
        Assert.Equal("andy@corp.example.com", user!.Email);
        Assert.True(user.EmailConfirmed);
        Assert.Equal($"{UserPrefix}andy", user.UserName);
    }

    [Fact]
    public async Task RepeatProvisioningIsIdempotent()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareAsync(connectionString);
        using var host = new ProvisionerHost(connectionString, new LdapOptions { AutoProvision = true });

        var info = Info("guid-2", $"{UserPrefix}repeat", "repeat@corp.example.com");

        await host.Provisioner.EnsureProvisionedAsync(info);
        await host.Provisioner.EnsureProvisionedAsync(info);

        var user = await host.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        Assert.NotNull(user);

        var logins = await host.UserManager.GetLoginsAsync(user!);
        Assert.Single(logins);
    }

    [Fact]
    public async Task MappedGroupGrantsRole()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareAsync(connectionString);

        var role = UniqueRole();
        var options = new LdapOptions { AutoProvision = true };
        options.GroupRoleMappings["Carnitas Admins"] = role;

        using var host = new ProvisionerHost(connectionString, options);

        var info = Info("guid-3", $"{UserPrefix}admin", "admin@corp.example.com",
            "CN=Carnitas Admins,OU=Groups,DC=corp,DC=example,DC=com");

        await host.Provisioner.EnsureProvisionedAsync(info);

        var user = await host.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        Assert.NotNull(user);
        Assert.True(await host.UserManager.IsInRoleAsync(user!, role));
    }

    [Fact]
    public async Task RemovedGroupRevokesMappedRoleButKeepsManualRole()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareAsync(connectionString);

        var mappedRole = UniqueRole();
        var manualRole = UniqueRole();
        var options = new LdapOptions { AutoProvision = true };
        options.GroupRoleMappings["Carnitas Admins"] = mappedRole;

        using var host = new ProvisionerHost(connectionString, options);

        var withGroup = Info("guid-4", $"{UserPrefix}revoke", "revoke@corp.example.com",
            "CN=Carnitas Admins,OU=Groups,DC=corp,DC=example,DC=com");
        await host.Provisioner.EnsureProvisionedAsync(withGroup);

        var user = await host.UserManager.FindByLoginAsync(withGroup.LoginProvider, withGroup.ProviderKey);
        Assert.NotNull(user);

        await host.RoleManager.CreateAsync(new IdentityRole(manualRole));
        await host.UserManager.AddToRoleAsync(user!, manualRole);
        Assert.True(await host.UserManager.IsInRoleAsync(user!, mappedRole));

        var withoutGroup = Info("guid-4", $"{UserPrefix}revoke", "revoke@corp.example.com");
        await host.Provisioner.EnsureProvisionedAsync(withoutGroup);

        var roles = await host.UserManager.GetRolesAsync(user!);
        Assert.DoesNotContain(mappedRole, roles);
        Assert.Contains(manualRole, roles);
    }

    [Fact]
    public async Task AutoProvisionDisabledLeavesUserUnlinked()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareAsync(connectionString);
        using var host = new ProvisionerHost(connectionString, new LdapOptions { AutoProvision = false });

        var info = Info("guid-5", $"{UserPrefix}manual", "manual@corp.example.com");

        await host.Provisioner.EnsureProvisionedAsync(info);

        Assert.Null(await host.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey));
    }

    [Fact]
    public async Task DirectoryUserDoesNotLinkToExistingEmailAccount()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareAsync(connectionString);
        using var host = new ProvisionerHost(connectionString, new LdapOptions { AutoProvision = true });

        var existing = new ApplicationUser
        {
            UserName = $"{UserPrefix}existing",
            Email = "clash@corp.example.com",
            EmailConfirmed = true
        };
        await host.UserManager.CreateAsync(existing);

        var info = Info("guid-6", $"{UserPrefix}clash", "clash@corp.example.com");
        await host.Provisioner.EnsureProvisionedAsync(info);

        var linked = await host.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        Assert.NotNull(linked);
        Assert.NotEqual(existing.Id, linked!.Id);
    }

    private static string UniqueRole() => $"LdapTestRole-{Guid.NewGuid():N}";

    private static ExternalLoginInfo Info(string objectGuid, string upn, string? email, params string[] groups)
    {
        var principal = LdapPrincipalFactory.Create(new LdapUser(
            objectGuid,
            "CN=Test,OU=Users,DC=corp,DC=example,DC=com",
            upn,
            email,
            "Test User",
            groups));

        return new ExternalLoginInfo(principal, LdapAuthenticationDefaults.Scheme, objectGuid, "Active Directory");
    }

    private static async Task PrepareAsync(string connectionString)
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options);

        await db.Database.EnsureCreatedAsync();

        await db.Database.ExecuteSqlRawAsync($"""
            DELETE FROM "AspNetUserRoles" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "UserName" LIKE '{UserPrefix}%');
            DELETE FROM "AspNetUserLogins" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "UserName" LIKE '{UserPrefix}%');
            DELETE FROM "AspNetUsers" WHERE "UserName" LIKE '{UserPrefix}%';
            """);
    }

    private static bool TryGetConnectionString(out string connectionString)
    {
        connectionString = Environment.GetEnvironmentVariable("CARNITAS_TEST_DB") ?? string.Empty;
        return !string.IsNullOrWhiteSpace(connectionString);
    }

    private sealed class ProvisionerHost : IDisposable
    {
        private readonly ServiceProvider _provider;

        public ProvisionerHost(string connectionString, LdapOptions options)
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(builder => builder.UseNpgsql(connectionString));
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
            services.AddScoped<ExternalUserProvisioner>();

            _provider = services.BuildServiceProvider();
        }

        public UserManager<ApplicationUser> UserManager => _provider.GetRequiredService<UserManager<ApplicationUser>>();

        public RoleManager<IdentityRole> RoleManager => _provider.GetRequiredService<RoleManager<IdentityRole>>();

        public ExternalUserProvisioner Provisioner => _provider.GetRequiredService<ExternalUserProvisioner>();

        public void Dispose() => _provider.Dispose();
    }
}
