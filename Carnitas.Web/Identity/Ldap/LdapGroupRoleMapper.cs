namespace Carnitas.Web.Identity.Ldap;

public sealed record RoleSyncPlan(IReadOnlyList<string> RolesToAdd, IReadOnlyList<string> RolesToRemove);

/// <summary>
/// Works out which roles managed by <see cref="LdapOptions.GroupRoleMappings"/> should be added or removed
/// for a directory user. Roles not named in the mappings are never touched, so manual assignments survive.
/// </summary>
public static class LdapGroupRoleMapper
{
    public static RoleSyncPlan Map(
        IEnumerable<string> userGroups,
        IReadOnlyDictionary<string, string> groupRoleMappings,
        IEnumerable<string> currentRoles)
    {
        var groups = Normalize(userGroups);
        var current = new HashSet<string>(
            currentRoles.Where(role => !string.IsNullOrWhiteSpace(role)),
            StringComparer.OrdinalIgnoreCase);

        var managedRoles = new HashSet<string>(
            groupRoleMappings.Values.Where(role => !string.IsNullOrWhiteSpace(role)),
            StringComparer.OrdinalIgnoreCase);

        var desiredRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (group, role) in groupRoleMappings)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(group))
            {
                continue;
            }

            if (groups.Overlaps(Normalize([group])))
            {
                desiredRoles.Add(role);
            }
        }

        var toAdd = desiredRoles.Where(role => !current.Contains(role)).ToList();
        var toRemove = managedRoles
            .Where(role => !desiredRoles.Contains(role) && current.Contains(role))
            .ToList();

        return new RoleSyncPlan(toAdd, toRemove);
    }

    /// <summary>
    /// True when the configured group (CN or DN) matches any of the user's groups (CN or DN).
    /// </summary>
    public static bool Matches(IEnumerable<string> userGroups, string group)
        => Normalize(userGroups).Overlaps(Normalize([group]));

    /// <summary>
    /// Indexes groups by their full value and, for DNs, their CN so a configured CN matches a DN and vice versa.
    /// </summary>
    private static HashSet<string> Normalize(IEnumerable<string> groups)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in groups)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                continue;
            }

            keys.Add(group);

            var cn = ExtractCn(group);
            if (cn is not null)
            {
                keys.Add(cn);
            }
        }

        return keys;
    }

    private static string? ExtractCn(string distinguishedName)
    {
        foreach (var part in distinguishedName.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
            {
                return part[3..];
            }
        }

        return null;
    }
}
