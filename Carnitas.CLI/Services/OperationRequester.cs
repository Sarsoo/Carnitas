using Carnitas.CLI.Operation;
using Carnitas.CLI.Options;
using Carnitas.Grpc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Carnitas.CLI.Services;

public class OperationRequester(
    Agent.AgentClient client, 
    OperationQueue queue,
    IOptions<BackendOptions> backendOptions, 
    IOptions<WorkerOptions> workerOptions, 
    ILogger<OperationRequester> logger
): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting work requester...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var resp = await client.RequestOperationAsync(new OperationRequest
                {
                    WorkerId = workerOptions.Value.Name
                }, cancellationToken:  stoppingToken);

                await queue.AddAsync(resp);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occured while requesting work");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(backendOptions.Value.WorkPollDelay), stoppingToken);
        }
    }
}