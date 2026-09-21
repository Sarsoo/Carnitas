using System.Threading.Channels;
using Carnitas.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Carnitas.Job;

public interface IJobDispatcher
{
    int JobCapacity { get; }
    int JobCount { get; }
    bool QueueJob(IJob job);
    Task Start(CancellationToken token);
}

public class JobDispatcher(IOptions<WorkerOptions> options, ILogger<JobDispatcher> logger) : BackgroundService, IJobDispatcher
{
    private readonly SemaphoreSlim _semaphore = new(options.Value.ConcurrentJobs, options.Value.ConcurrentJobs);
    private readonly Channel<IJob> _channel = Channel.CreateUnbounded<IJob>();

    public int JobCapacity => _semaphore.CurrentCount;
    public int JobCount => _channel.Reader.Count;

    public bool QueueJob(IJob job) => _channel.Writer.TryWrite(job);

    public async Task Start(CancellationToken token)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(token).ConfigureAwait(false))
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);
            Task.Run(async () =>
            {
                try
                {
                    logger.LogInformation("Starting job {job}", job.Id);
                    await job.Execute(token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Execution of job {job} failed", job.Id);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Start(stoppingToken);
}