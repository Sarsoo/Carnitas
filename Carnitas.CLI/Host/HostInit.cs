using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Carnitas.CLI.Host;

public sealed record CliArgs(string[] Args);

public class HostInit
{
    public static HostApplicationBuilder Init(string[] args)
    {
        var settings = new HostApplicationBuilderSettings();
        var host = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(settings);

        host.Logging.ClearProviders().AddNLog();

        host.Services.AddSingleton(new CliArgs(args));

        return host;
    }
}