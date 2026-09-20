using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Carnitas.CLI.Operation;

public class OperationDispatcher(
    OperationQueue queue,
    ILogger<OperationDispatcher> logger
): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting queue processing of received operations..");
        await foreach (var work in queue.Reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var logScope = logger.BeginScope(
                    new[]
                    {
                        new KeyValuePair<string, object>("id", work.Id),
                        new KeyValuePair<string, object>("repo", work.Repo),
                        new KeyValuePair<string, object>("module", work.ModulePath)
                    });
                logger.LogInformation("Processing operation");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occured while executing work");
            }
        }
    }
}