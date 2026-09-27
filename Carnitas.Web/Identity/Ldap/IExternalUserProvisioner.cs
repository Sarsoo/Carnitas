using Microsoft.AspNetCore.Identity;

namespace Carnitas.Web.Identity.Ldap;

public interface IExternalUserProvisioner
{
    /// <summary>
    /// Finds or creates the local account for an external login and syncs directory-driven roles.
    /// A no-op for providers other than LDAP.
    /// </summary>
    Task EnsureProvisionedAsync(ExternalLoginInfo info, CancellationToken cancellationToken = default);
}
