using Carnitas.Model.Identity;
using Microsoft.AspNetCore.Identity;

namespace Carnitas.Web.Identity.Ldap;

/// <summary>Helpers for deciding whether a local account's credentials are owned by the directory.</summary>
public static class LdapAccount
{
    /// <summary>
    /// True when the user has no local password and an LDAP login: their credentials are directory-managed,
    /// so local password management must be blocked.
    /// </summary>
    public static async Task<bool> IsDirectoryManagedAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
    {
        if (await userManager.HasPasswordAsync(user))
        {
            return false;
        }

        var logins = await userManager.GetLoginsAsync(user);

        return logins.Any(login =>
            string.Equals(login.LoginProvider, LdapAuthenticationDefaults.Scheme, StringComparison.OrdinalIgnoreCase));
    }
}
