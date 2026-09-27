namespace Carnitas.Web.Identity.Ldap;

/// <summary>
/// Configuration for Active Directory / LDAP sign-in. Bound from the "Ldap" configuration section.
/// </summary>
public sealed class LdapOptions
{
    public const string SectionName = "Ldap";

    /// <summary>When false the LDAP authentication scheme is not registered and the external login picker stays empty.</summary>
    public bool Enabled { get; set; }

    /// <summary>Display name shown on the external login picker button.</summary>
    public string DisplayName { get; set; } = "Active Directory";

    public string Host { get; set; } = "";

    public int Port { get; set; } = 636;

    /// <summary>Use LDAPS. When false the connection is plain LDAP.</summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>Skip LDAPS server certificate validation. Intended only for development/testing.</summary>
    public bool SkipCertificateValidation { get; set; }

    public string BaseDn { get; set; } = "";

    /// <summary>User search filter; <c>{0}</c> is replaced with the escaped username.</summary>
    public string UserSearchFilter { get; set; } = "(&(objectClass=user)(sAMAccountName={0}))";

    /// <summary>Optional service account used to search the directory. When empty the user's credentials are used to bind directly.</summary>
    public string? ServiceAccountDn { get; set; }

    public string? ServiceAccountPassword { get; set; }

    /// <summary>Suffix appended to the username for a direct UPN bind when no service account is configured.</summary>
    public string? UpnSuffix { get; set; }

    /// <summary>Expand direct group membership with the AD nested-group matching rule.</summary>
    public bool ResolveNestedGroups { get; set; } = true;

    /// <summary>Optional group (CN or DN) a user must be a member of to sign in.</summary>
    public string? RequiredGroup { get; set; }

    /// <summary>Create and link a local user automatically on first successful directory login.</summary>
    public bool AutoProvision { get; set; } = true;

    public int ConnectionTimeoutSeconds { get; set; } = 10;

    /// <summary>Maps directory group (CN or DN) to a local Identity role name.</summary>
    public Dictionary<string, string> GroupRoleMappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
