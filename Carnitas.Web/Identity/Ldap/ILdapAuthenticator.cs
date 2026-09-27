namespace Carnitas.Web.Identity.Ldap;

public interface ILdapAuthenticator
{
    Task<LdapAuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
