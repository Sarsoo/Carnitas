namespace Carnitas.Web.Identity.Ldap;

public static class LdapClaimTypes
{
    public const string ObjectGuid = "ldap:objectGuid";
    public const string DistinguishedName = "ldap:distinguishedName";
    public const string UserPrincipalName = "ldap:userPrincipalName";

    /// <summary>One claim per directory group (DN and, where applicable, CN).</summary>
    public const string Group = "ldap:group";
}
