using System.Security.Claims;
using Carnitas.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Carnitas.Web.Identity.Ldap;

/// <summary>
/// Creates and maintains the local <see cref="ApplicationUser"/> behind an LDAP login, and keeps the
/// directory-driven Identity roles in sync. Only ever acts on the LDAP provider.
/// </summary>
public sealed class ExternalUserProvisioner(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<LdapOptions> options,
    ILogger<ExternalUserProvisioner> logger) : IExternalUserProvisioner
{
    private readonly LdapOptions _options = options.Value;

    public async Task EnsureProvisionedAsync(ExternalLoginInfo info, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(info.LoginProvider, LdapAuthenticationDefaults.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        if (user is null)
        {
            if (!_options.AutoProvision)
            {
                return;
            }

            user = await CreateUserAsync(info);
            if (user is null)
            {
                return;
            }
        }
        else
        {
            user = await RefreshDirectoryEmailAsync(user, info.Principal);
        }

        await SyncRolesAsync(user, info.Principal);
    }

    private async Task<ApplicationUser?> CreateUserAsync(ExternalLoginInfo info)
    {
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var username = info.Principal.FindFirstValue(LdapClaimTypes.UserPrincipalName)
                       ?? info.Principal.FindFirstValue(ClaimTypes.Name)
                       ?? email;

        if (string.IsNullOrWhiteSpace(username))
        {
            logger.LogWarning("Cannot provision LDAP user: the directory supplied no username or email.");
            return null;
        }

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            // Directory-authenticated accounts never wait on the (no-op) email sender.
            EmailConfirmed = true
        };

        try
        {
            var create = await userManager.CreateAsync(user);
            if (!create.Succeeded)
            {
                // A concurrent first sign-in may already have created and linked the account.
                var existing = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (existing is not null)
                {
                    return existing;
                }

                logger.LogError("Failed to provision LDAP user: {Errors}",
                    string.Join(", ", create.Errors.Select(error => error.Description)));
                return null;
            }

            var link = await userManager.AddLoginAsync(user, info);
            if (!link.Succeeded)
            {
                logger.LogError("Failed to link LDAP login for user {UserId}: {Errors}", user.Id,
                    string.Join(", ", link.Errors.Select(error => error.Description)));
                return null;
            }

            logger.LogInformation("Provisioned local account for directory user {Username}.", username);
            return user;
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "Concurrent LDAP provisioning detected; re-reading the linked account.");
            return await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }
    }

    private async Task<ApplicationUser> RefreshDirectoryEmailAsync(ApplicationUser user, ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email) || string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            return user;
        }

        user.Email = email;
        user.EmailConfirmed = true;

        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            logger.LogWarning("Failed to refresh directory email for user {UserId}: {Errors}", user.Id,
                string.Join(", ", update.Errors.Select(error => error.Description)));
        }

        return user;
    }

    private async Task SyncRolesAsync(ApplicationUser user, ClaimsPrincipal principal)
    {
        if (_options.GroupRoleMappings.Count == 0)
        {
            return;
        }

        var groups = principal.FindAll(LdapClaimTypes.Group).Select(claim => claim.Value).ToList();
        var currentRoles = await userManager.GetRolesAsync(user);
        var plan = LdapGroupRoleMapper.Map(groups, _options.GroupRoleMappings, currentRoles);

        foreach (var role in plan.RolesToAdd)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var create = await roleManager.CreateAsync(new IdentityRole(role));
                if (!create.Succeeded)
                {
                    logger.LogError("Failed to create role {Role} required by the LDAP mapping: {Errors}", role,
                        string.Join(", ", create.Errors.Select(error => error.Description)));
                    continue;
                }
            }

            var add = await userManager.AddToRoleAsync(user, role);
            if (!add.Succeeded)
            {
                logger.LogWarning("Failed to add role {Role} to user {UserId}: {Errors}", role, user.Id,
                    string.Join(", ", add.Errors.Select(error => error.Description)));
            }
        }

        foreach (var role in plan.RolesToRemove)
        {
            var remove = await userManager.RemoveFromRoleAsync(user, role);
            if (!remove.Succeeded)
            {
                logger.LogWarning("Failed to remove role {Role} from user {UserId}: {Errors}", role, user.Id,
                    string.Join(", ", remove.Errors.Select(error => error.Description)));
            }
        }
    }
}
