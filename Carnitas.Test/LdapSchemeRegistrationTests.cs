using System.Net;
using System.Text.RegularExpressions;
using Carnitas.Web.Identity.Ldap;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Carnitas.Test;

public class LdapSchemeRegistrationTests
{
    [Fact]
    public async Task LdapSchemeIsRegisteredWhenEnabled()
    {
        using var environment = new EnvironmentVariableScope(("Ldap__Enabled", "true"), ("Ldap__DisplayName", "Active Directory"));
        using var factory = new WebApplicationFactory<Program>();

        var scheme = await GetSchemeAsync(factory);

        Assert.NotNull(scheme);
        Assert.Equal("Active Directory", scheme!.DisplayName);
    }

    [Fact]
    public async Task LdapSchemeIsAbsentWhenDisabled()
    {
        using var environment = new EnvironmentVariableScope(("Ldap__Enabled", "false"));
        using var factory = new WebApplicationFactory<Program>();

        var scheme = await GetSchemeAsync(factory);

        Assert.Null(scheme);
    }

    [Fact]
    public async Task LoginPageShowsDirectoryProviderWhenEnabled()
    {
        using var environment = new EnvironmentVariableScope(("Ldap__Enabled", "true"), ("Ldap__DisplayName", "Active Directory"));
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("Active Directory", html);
    }

    [Fact]
    public async Task ChallengeRedirectsToCredentialsPage()
    {
        using var environment = new EnvironmentVariableScope(("Ldap__Enabled", "true"), ("Ldap__DisplayName", "Active Directory"));
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var loginPage = await client.GetAsync("/Account/Login");
        var token = ExtractAntiforgeryToken(await loginPage.Content.ReadAsStringAsync());

        var response = await client.PostAsync("/Account/PerformExternalLogin", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["provider"] = LdapAuthenticationDefaults.Scheme,
                ["returnUrl"] = "/"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/LdapLogin", response.Headers.Location?.ToString());
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"(?<value>[^\"]+)\"", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            match = Regex.Match(html, "<input[^>]*value=\"(?<value>[^\"]+)\"[^>]*name=\"__RequestVerificationToken\"", RegexOptions.IgnoreCase);
        }

        Assert.True(match.Success, "Antiforgery token not found on the login page.");
        return match.Groups["value"].Value;
    }

    [Fact]
    public async Task CredentialsPageRendersWhenEnabled()
    {
        using var environment = new EnvironmentVariableScope(("Ldap__Enabled", "true"), ("Ldap__DisplayName", "Active Directory"));
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Account/LdapLogin");
        var html = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("Log in with Active Directory", html);
    }

    [Fact]
    public async Task LoginPageHidesDirectoryProviderWhenDisabled()
    {
        using var environment = new EnvironmentVariableScope(("Ldap__Enabled", "false"));
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("no external authentication services configured", html);
    }

    private static async Task<AuthenticationScheme?> GetSchemeAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        return await provider.GetSchemeAsync(LdapAuthenticationDefaults.Scheme);
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly Dictionary<string, string?> _original = new();

        public EnvironmentVariableScope(params (string Key, string Value)[] values)
        {
            foreach (var (key, value) in values)
            {
                _original[key] = Environment.GetEnvironmentVariable(key);
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        public void Dispose()
        {
            foreach (var (key, value) in _original)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
