using System.CommandLine;
using Carnitas.CLI.Host;
using Carnitas.CLI.Operation;
using Carnitas.CLI.Options;
using Carnitas.CLI.Services;
using Carnitas.Grpc;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Carnitas.CLI.Command;

public class Start: System.CommandLine.Command
{

    public Start()
        : base("start", "Start worker")
    {
        SetAction(Run);
    }

    private async Task<int> Run(ParseResult parseResult)
    {
        var host = HostInit.Init();
        
        host.Services.Configure<BackendOptions>(host.Configuration.GetSection(BackendOptions.Key));
        host.Services.Configure<WorkerOptions>(host.Configuration.GetSection(WorkerOptions.Key));
        
        var options = host.Configuration.GetSection(BackendOptions.Key).Get<BackendOptions>();

        host.Services.AddSingleton<ChannelBase>(sp => GrpcChannel.ForAddress(options.Url));
        host.Services.AddTransient<Agent.AgentClient>();

        host.Services.AddSingleton<OperationQueue>()
            .AddHostedService<OperationRequester>()
            .AddHostedService<OperationDispatcher>();

        await host.Build().RunAsync();

        return 0;
    }
}