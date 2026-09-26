using System.CommandLine;
using Carnitas.CLI.Backend;
using Carnitas.CLI.Host;
using Carnitas.CLI.Operation;
using Carnitas.CLI.Options;
using Carnitas.CLI.Services;
using Carnitas.Extensions;
using Carnitas.Grpc;
using Carnitas.Options;
using Carnitas.Workflow.Output;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

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
            .AddHostedService<OperationDispatcher>()
            .AddSingleton<IPlanReporter, PlanReporter>()
            .AddSingleton<IStatusReporter, AgentStatusReporter>()
            .AddSingleton<ILogReporter, AgentLogReporter>()
            .AddSingleton<IRootModuleReporter, AgentRootModuleReporter>()
            .AddCarnitas(host.Configuration);
        
        ///////////////////////
        //   OBSERVABILITY
        ///////////////////////

        host.Services.AddOpenTelemetry()
            .WithLogging(b =>
            {
                b.AddOtlpExporter();
            })
            .WithMetrics(b =>
            {
                b.AddMeter("Sarsoo.*");
                b.AddMeter("Carnitas.*");
                b.AddOtlpExporter();
            })
            .WithTracing(b =>
            {
                b.AddSource("Sarsoo.*");
                b.AddSource("Carnitas.*");
                b.AddOtlpExporter();
            });

        await host.Build().RunAsync();

        return 0;
    }
}