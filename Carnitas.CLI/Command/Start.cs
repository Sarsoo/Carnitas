using System.CommandLine;
using Carnitas.CLI.Backend;
using Carnitas.CLI.Host;
using Carnitas.CLI.Operation;
using Carnitas.CLI.Options;
using Carnitas.CLI.Services;
using Carnitas.Extensions;
using Carnitas.Grpc;
using Carnitas.Observability;
using Carnitas.Options;
using Carnitas.Workflow.Output;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
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

        host.Services.AddServiceDiscovery();

        host.Services.AddGrpcClient<Agent.AgentClient>(o =>
        {
            o.Address = new Uri("http://backend");
        }).AddServiceDiscovery();

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
            .UseOtlpExporter()
            .WithLogging(b =>
            {

            })
            .WithMetrics(b =>
            {
                b.AddMeter("Sarsoo.*");
                b.AddMeter("Carnitas.*");
                
                b.AddRuntimeInstrumentation();
            })
            .WithTracing(b =>
            {
                b.AddSource("Sarsoo.*");
                b.AddSource("Carnitas.*");
                
                b.AddGrpcClientInstrumentation();
            });
        BaggageTagMapper.MapBaggageToTags();
        UnhandledExceptionHandler.SetUnhandledExceptionHandler();

        await host.Build().RunAsync();

        return 0;
    }
}