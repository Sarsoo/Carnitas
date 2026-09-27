using System.Security.Claims;

namespace Carnitas.Web.Identity.Ldap;

/// <summary>Builds the claims principal stored in the Identity external cookie for a directory user.</summary>
public static class LdapPrincipalFactory
{
    public static ClaimsPrincipal Create(LdapUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.ObjectGuid),
            new(ClaimTypes.Name, user.UserPrincipalName),
            new(ClaimTypes.Email, user.Email ?? user.UserPrincipalName),
            new(LdapClaimTypes.ObjectGuid, user.ObjectGuid),
            new(LdapClaimTypes.DistinguishedName, user.DistinguishedName),
            new(LdapClaimTypes.UserPrincipalName, user.UserPrincipalName),
        };

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            claims.Add(new Claim(ClaimTypes.GivenName, user.DisplayName));
        }

        foreach (var group in user.Groups
                     .Where(group => !string.IsNullOrWhiteSpace(group))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(LdapClaimTypes.Group, group));
        }

        var identity = new ClaimsIdentity(
            claims,
            LdapAuthenticationDefaults.Scheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }
}
