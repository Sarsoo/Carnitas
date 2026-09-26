using Carnitas.Model.Operations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Carnitas.Web.Grpc;

public class TaskQueueMaintenanceService(
    IServiceScopeFactory scopeFactory,
    TaskQueueOptions options,
    ILogger<TaskQueueMaintenanceService> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.SweepIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, stoppingToken).ConfigureAwait(false);

                using var scope = scopeFactory.CreateScope();
                var queue = scope.ServiceProvider.GetRequiredService<ITaskQueue>();

                var swept = await queue.SweepExpiredAsync(stoppingToken).ConfigureAwait(false);

                if (swept > 0)
                {
                    logger.LogWarning("Marked {count} expired queue tasks as failed", swept);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while sweeping the task queue");
            }
        }
    }
}
