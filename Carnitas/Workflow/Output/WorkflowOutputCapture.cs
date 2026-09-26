using System.Threading.Channels;
using Carnitas.Workflow.Stage;
using Microsoft.Extensions.Hosting;

namespace Carnitas.Workflow.Output;

public class WorkflowOutputCapture(
    ILogReporter? logReporter = null
): BackgroundService, IWorkflowOutputCapture
{
    private const int BatchSize = 50;

    private readonly Channel<IStage> _stageQueue = Channel.CreateUnbounded<IStage>();

    public ValueTask AddStage(IStage stage)
    {
        return _stageQueue.Writer.WriteAsync(stage);
    }

    public async Task Process(CancellationToken cancel = default)
    {
        await foreach (var stage in _stageQueue.Reader.ReadAllAsync(cancel).ConfigureAwait(false))
        {
            await Task.WhenAll(
                ProcessStageMessages(stage, cancel),
                ProcessStageJson(stage, cancel)
            ).ConfigureAwait(false);
        }
    }

    private async Task ProcessStageMessages(IStage stage, CancellationToken cancel)
    {
        if (stage.MessageOutput is null)
        {
            return;
        }

        var batch = new List<StageLogLine>(BatchSize);

        await foreach (var message in stage.MessageOutput.ReadAllAsync(cancel).ConfigureAwait(false))
        {
            batch.Add(new StageLogLine(stage.Id, message?.ToString() ?? string.Empty, "Information", "Message"));

            if (batch.Count >= BatchSize)
            {
                await Flush(batch, cancel).ConfigureAwait(false);
            }
        }

        await Flush(batch, cancel).ConfigureAwait(false);
    }

    private async Task ProcessStageJson(IStage stage, CancellationToken cancel)
    {
        if (stage.JsonOutput is null)
        {
            return;
        }

        var batch = new List<StageLogLine>(BatchSize);

        await foreach (var line in stage.JsonOutput.ReadAllAsync(cancel).ConfigureAwait(false))
        {
            batch.Add(new StageLogLine(stage.Id, line ?? string.Empty, "Information", "Json"));

            if (batch.Count >= BatchSize)
            {
                await Flush(batch, cancel).ConfigureAwait(false);
            }
        }

        await Flush(batch, cancel).ConfigureAwait(false);
    }

    private async Task Flush(List<StageLogLine> batch, CancellationToken cancel)
    {
        if (batch.Count == 0)
        {
            return;
        }

        if (logReporter is null)
        {
            batch.Clear();
            return;
        }

        try
        {
            await logReporter.ReportLogs(batch.ToList(), cancel).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Log reporting must never break workflow execution.
        }

        batch.Clear();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Process(stoppingToken);
}
