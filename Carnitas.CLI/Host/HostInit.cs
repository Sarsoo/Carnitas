using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Carnitas.CLI.Host;

public class HostInit
{
    public static HostApplicationBuilder Init()
    {
        var settings = new HostApplicationBuilderSettings();
        var host = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(settings);
        
        host.Configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        host.Logging.ClearProviders().AddNLog(host.Configuration);

        return host;
    }
}