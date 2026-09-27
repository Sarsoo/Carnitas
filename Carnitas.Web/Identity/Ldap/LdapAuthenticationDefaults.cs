namespace Carnitas.Web.Identity.Ldap;

public static class LdapAuthenticationDefaults
{
    /// <summary>The external authentication scheme name used for directory sign-in.</summary>
    public const string Scheme = "LDAP";

    /// <summary>Authentication properties item key carrying the external login provider name.</summary>
    public const string LoginProviderKey = "LoginProvider";

    /// <summary>Authentication properties item key carrying the external login provider display name.</summary>
    public const string ProviderDisplayNameKey = "LoginProviderDisplayName";

    /// <summary>Authentication properties item key carrying the XSRF id used when linking a login to an account.</summary>
    public const string XsrfKey = "XsrfId";
}
