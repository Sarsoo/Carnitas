namespace Carnitas.Web.Identity.Ldap;

/// <summary>A directory account resolved from a successful LDAP bind.</summary>
public sealed record LdapUser(
    string ObjectGuid,
    string DistinguishedName,
    string UserPrincipalName,
    string? Email,
    string? DisplayName,
    IReadOnlyList<string> Groups);

public sealed record LdapAuthenticationResult(bool Succeeded, LdapUser? User, string? FailureReason)
{
    public static LdapAuthenticationResult Success(LdapUser user) => new(true, user, null);

    public static LdapAuthenticationResult Failure(string reason) => new(false, null, reason);
}
