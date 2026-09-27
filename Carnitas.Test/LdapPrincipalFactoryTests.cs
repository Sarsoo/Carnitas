using System.Security.Claims;
using Carnitas.Web.Identity.Ldap;

namespace Carnitas.Test;

public class LdapPrincipalFactoryTests
{
    private static LdapUser User(
        string objectGuid = "8f2b0a2e-0000-0000-0000-000000000001",
        string upn = "andy@corp.example.com",
        string? email = "andy@corp.example.com",
        params string[] groups) =>
        new(objectGuid, "CN=Andy,OU=Users,DC=corp,DC=example,DC=com", upn, email, "Andy Smith", groups);

    [Fact]
    public void UsesObjectGuidAsNameIdentifier()
    {
        var principal = LdapPrincipalFactory.Create(User());

        Assert.Equal("8f2b0a2e-0000-0000-0000-000000000001", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(LdapAuthenticationDefaults.Scheme, principal.Identity?.AuthenticationType);
    }

    [Fact]
    public void FallsBackToUpnWhenEmailIsMissing()
    {
        var principal = LdapPrincipalFactory.Create(User(email: null));

        Assert.Equal("andy@corp.example.com", principal.FindFirstValue(ClaimTypes.Email));
    }

    [Fact]
    public void EmitsOneDistinctGroupClaimPerGroup()
    {
        var principal = LdapPrincipalFactory.Create(User(
            groups: ["CN=Carnitas Admins,OU=Groups,DC=corp,DC=example,DC=com", "cn=carnitas admins,ou=groups,dc=corp,dc=example,dc=com", ""]));

        var groups = principal.FindAll(LdapClaimTypes.Group).Select(claim => claim.Value).ToList();

        Assert.Single(groups);
        Assert.Equal("CN=Carnitas Admins,OU=Groups,DC=corp,DC=example,DC=com", groups[0]);
    }
}
