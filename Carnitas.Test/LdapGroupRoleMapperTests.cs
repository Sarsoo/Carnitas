using Carnitas.Web.Identity.Ldap;

namespace Carnitas.Test;

public class LdapGroupRoleMapperTests
{
    private static readonly Dictionary<string, string> Mappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Carnitas Admins"] = "Admin",
        ["CN=Terraform Users,OU=Groups,DC=corp,DC=example,DC=com"] = "PowerUser"
    };

    [Fact]
    public void AddsRoleForMatchingGroup()
    {
        var plan = LdapGroupRoleMapper.Map(
            ["CN=Carnitas Admins,OU=Groups,DC=corp,DC=example,DC=com"],
            Mappings,
            []);

        Assert.Equal(["Admin"], plan.RolesToAdd);
        Assert.Empty(plan.RolesToRemove);
    }

    [Fact]
    public void MatchesConfiguredCnAgainstDirectoryDnAndViceVersa()
    {
        var fromDn = LdapGroupRoleMapper.Map(["CN=Carnitas Admins,OU=Groups,DC=corp,DC=example,DC=com"], Mappings, []);
        var fromCn = LdapGroupRoleMapper.Map(["Terraform Users"], Mappings, []);

        Assert.Contains("Admin", fromDn.RolesToAdd);
        Assert.Contains("PowerUser", fromCn.RolesToAdd);
    }

    [Fact]
    public void RemovesManagedRoleThatIsNoLongerGranted()
    {
        var plan = LdapGroupRoleMapper.Map([], Mappings, ["Admin"]);

        Assert.Empty(plan.RolesToAdd);
        Assert.Equal(["Admin"], plan.RolesToRemove);
    }

    [Fact]
    public void LeavesManuallyAssignedRolesAlone()
    {
        var plan = LdapGroupRoleMapper.Map([], Mappings, ["ManualReviewer"]);

        Assert.Empty(plan.RolesToAdd);
        Assert.Empty(plan.RolesToRemove);
    }

    [Fact]
    public void IsCaseInsensitive()
    {
        var plan = LdapGroupRoleMapper.Map(["carnitas admins"], Mappings, ["admin"]);

        Assert.Empty(plan.RolesToAdd);
        Assert.Empty(plan.RolesToRemove);
    }
}
