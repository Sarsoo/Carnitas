using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Carnitas.Web.Identity.Ldap;

public sealed class LdapAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>Local page that collects directory credentials when the scheme is challenged.</summary>
    public string LoginPath { get; set; } = "/Account/LdapLogin";
}

/// <summary>
/// Marker scheme that lets Active Directory participate in the Identity external-login flow. It never
/// authenticates a request itself: a challenge redirects to the credentials page, which then creates the
/// Identity external cookie via <see cref="LdapPrincipalFactory"/>.
/// </summary>
public sealed class LdapAuthenticationHandler(
    IOptionsMonitor<LdapAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<LdapAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var query = new Dictionary<string, string?>
        {
            ["ReturnUrl"] = properties.RedirectUri ?? "/"
        };

        // Preserve the XSRF id used by the "link an external login" flow on the Account/Manage page.
        if (properties.Items.TryGetValue(LdapAuthenticationDefaults.XsrfKey, out var xsrf) && !string.IsNullOrEmpty(xsrf))
        {
            query[LdapAuthenticationDefaults.XsrfKey] = xsrf;
        }

        Response.Redirect(QueryHelpers.AddQueryString(Options.LoginPath, query!));

        return Task.CompletedTask;
    }
}
