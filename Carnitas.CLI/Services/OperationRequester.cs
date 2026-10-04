using Carnitas.CLI.Operation;
using Carnitas.CLI.Options;
using Carnitas.Grpc;
using Carnitas.Options;
using Grpc.Core;
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
                logger.LogDebug("Requesting operation from backend");
                var resp = await client.RequestWorkflowAsync(new OperationRequest
                {
                    WorkerId = workerOptions.Value.Name
                }, cancellationToken:  stoppingToken).ConfigureAwait(false);

                if (resp is { HasWork: true } && resp.Operations.Count > 0)
                {
                    await queue.AddAsync(resp).ConfigureAwait(false);
                }
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Unavailable)
                {
                    logger.LogWarning("Web backend is currently unavailable");
                }
                else
                {
                    logger.LogError(e, "RPC exception while requesting work");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occured while requesting work");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(backendOptions.Value.WorkPollDelay), stoppingToken).ConfigureAwait(false);
        }
    }
}