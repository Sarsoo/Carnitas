using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;

namespace Carnitas.Web.Identity.Ldap;

/// <summary>
/// Authenticates a user against Active Directory / LDAP by binding with their credentials and reading their
/// directory entry. Directory errors are logged; callers only ever see a uniform failure message.
/// </summary>
public sealed class LdapAuthenticator(
    IOptions<LdapOptions> options,
    ILogger<LdapAuthenticator> logger) : ILdapAuthenticator
{
    private const string InvalidCredentialsMessage = "Invalid username or password.";

    private static readonly string[] UserAttributes =
    [
        "objectGUID",
        "sAMAccountName",
        "userPrincipalName",
        "mail",
        "displayName",
        "memberOf",
        "userAccountControl"
    ];

    private readonly LdapOptions _options = options.Value;

    public Task<LdapAuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Task.FromResult(LdapAuthenticationResult.Failure(InvalidCredentialsMessage));
        }

        // A simple bind with an empty password is an unauthenticated bind, which AD treats as success.
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return Task.FromResult(LdapAuthenticationResult.Failure(InvalidCredentialsMessage));
        }

        return Task.Run(() => Authenticate(username.Trim(), password), cancellationToken);
    }

    private LdapAuthenticationResult Authenticate(string username, string password)
    {
        try
        {
            using var connection = CreateConnection();

            SearchResultEntry entry;
            if (!string.IsNullOrWhiteSpace(_options.ServiceAccountDn))
            {
                connection.Bind(new NetworkCredential(_options.ServiceAccountDn, _options.ServiceAccountPassword));

                entry = FindUserEntry(connection, username, null)
                        ?? throw new LdapAuthenticationFailure("User not found.");

                ValidateCredentials(entry.DistinguishedName, password);
            }
            else
            {
                var bindIdentity = ResolveBindIdentity(username)
                                   ?? throw new LdapAuthenticationFailure("No UPN suffix or service account configured.");

                connection.Bind(new NetworkCredential(bindIdentity, password));

                entry = FindUserEntry(connection, username, bindIdentity)
                        ?? throw new LdapAuthenticationFailure("User not found.");
            }

            if (!IsAccountEnabled(entry))
            {
                throw new LdapAuthenticationFailure("Account is disabled.");
            }

            var groups = ResolveGroups(connection, entry);

            if (!string.IsNullOrWhiteSpace(_options.RequiredGroup) &&
                !LdapGroupRoleMapper.Matches(groups, _options.RequiredGroup!))
            {
                throw new LdapAuthenticationFailure("User is not a member of the required group.");
            }

            var user = new LdapUser(
                GetObjectGuid(entry),
                entry.DistinguishedName,
                GetString(entry, "userPrincipalName") ?? username,
                GetString(entry, "mail"),
                GetString(entry, "displayName"),
                groups);

            return LdapAuthenticationResult.Success(user);
        }
        catch (LdapAuthenticationFailure failure)
        {
            logger.LogInformation("LDAP sign-in failed: {Reason}", failure.Message);
            return LdapAuthenticationResult.Failure(InvalidCredentialsMessage);
        }
        catch (LdapException exception)
        {
            if (exception.ErrorCode == 49)
            {
                logger.LogInformation("LDAP sign-in failed: invalid credentials.");
            }
            else
            {
                logger.LogWarning(exception, "LDAP sign-in failed with directory error {ErrorCode}.", exception.ErrorCode);
            }

            return LdapAuthenticationResult.Failure(InvalidCredentialsMessage);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "LDAP sign-in failed unexpectedly.");
            return LdapAuthenticationResult.Failure(InvalidCredentialsMessage);
        }
    }

    private LdapConnection CreateConnection()
    {
        var identifier = new LdapDirectoryIdentifier(_options.Host, _options.Port, fullyQualifiedDnsHostName: true, connectionless: false);

        var connection = new LdapConnection(identifier)
        {
            AuthType = AuthType.Basic,
            Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.ConnectionTimeoutSeconds))
        };

        connection.SessionOptions.ProtocolVersion = 3;

        if (_options.UseSsl)
        {
            connection.SessionOptions.SecureSocketLayer = true;
        }

        if (_options.SkipCertificateValidation)
        {
            connection.SessionOptions.VerifyServerCertificate = (_, _) => true;
        }

        return connection;
    }

    private void ValidateCredentials(string distinguishedName, string password)
    {
        using var connection = CreateConnection();
        connection.Bind(new NetworkCredential(distinguishedName, password));
    }

    private SearchResultEntry? FindUserEntry(LdapConnection connection, string username, string? bindIdentity)
    {
        var filter = _options.UserSearchFilter.Replace("{0}", EscapeFilter(username), StringComparison.Ordinal);
        var entry = Search(connection, filter);
        if (entry is not null)
        {
            return entry;
        }

        if (!string.IsNullOrWhiteSpace(bindIdentity))
        {
            return Search(connection, $"(userPrincipalName={EscapeFilter(bindIdentity)})");
        }

        return null;
    }

    private SearchResultEntry? Search(LdapConnection connection, string filter)
    {
        var request = new SearchRequest(_options.BaseDn, filter, SearchScope.Subtree, UserAttributes);
        var response = (SearchResponse) connection.SendRequest(request);

        return response.Entries.Count > 0 ? response.Entries[0] : null;
    }

    private List<string> ResolveGroups(LdapConnection connection, SearchResultEntry entry)
    {
        var groups = new HashSet<string>(GetValues(entry, "memberOf"), StringComparer.OrdinalIgnoreCase);

        if (_options.ResolveNestedGroups && groups.Count > 0)
        {
            foreach (var groupDn in groups.ToList())
            {
                var filter = $"(member:1.2.840.113556.1.4.1941:={EscapeFilter(groupDn)})";
                var request = new SearchRequest(_options.BaseDn, filter, SearchScope.Subtree, "distinguishedName");

                if (connection.SendRequest(request) is SearchResponse response)
                {
                    foreach (SearchResultEntry result in response.Entries)
                    {
                        groups.Add(result.DistinguishedName);
                    }
                }
            }
        }

        return groups.ToList();
    }

    private string? ResolveBindIdentity(string username)
    {
        if (username.Contains('@'))
        {
            return username;
        }

        return string.IsNullOrWhiteSpace(_options.UpnSuffix) ? null : $"{username}@{_options.UpnSuffix}";
    }

    private static bool IsAccountEnabled(SearchResultEntry entry)
    {
        var value = GetString(entry, "userAccountControl");
        if (!int.TryParse(value, out var userAccountControl))
        {
            return true;
        }

        return (userAccountControl & 0x2) == 0;
    }

    private static string GetObjectGuid(SearchResultEntry entry)
    {
        var attribute = entry.Attributes["objectGUID"];
        if (attribute is not null && attribute.Count > 0)
        {
            var values = attribute.GetValues(typeof(byte[]));
            if (values.Length > 0 && values[0] is byte[] bytes && bytes.Length == 16)
            {
                return new Guid(bytes).ToString();
            }
        }

        return entry.DistinguishedName;
    }

    private static string? GetString(SearchResultEntry entry, string attributeName)
    {
        var attribute = entry.Attributes[attributeName];
        if (attribute is null || attribute.Count == 0)
        {
            return null;
        }

        return attribute[0]?.ToString();
    }

    private static IEnumerable<string> GetValues(SearchResultEntry entry, string attributeName)
    {
        var attribute = entry.Attributes[attributeName];
        if (attribute is null)
        {
            yield break;
        }

        foreach (var value in attribute.GetValues(typeof(string)))
        {
            if (value is string text && !string.IsNullOrWhiteSpace(text))
            {
                yield return text;
            }
        }
    }

    private static string EscapeFilter(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            switch (character)
            {
                case '\\':
                    builder.Append("\\5c");
                    break;
                case '*':
                    builder.Append("\\2a");
                    break;
                case '(':
                    builder.Append("\\28");
                    break;
                case ')':
                    builder.Append("\\29");
                    break;
                case '\0':
                    builder.Append("\\00");
                    break;
                default:
                    builder.Append(character);
                    break;
            }
        }

        return builder.ToString();
    }

    private sealed class LdapAuthenticationFailure(string message) : Exception(message);
}
