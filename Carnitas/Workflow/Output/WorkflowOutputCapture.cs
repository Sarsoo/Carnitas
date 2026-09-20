using System.Threading.Channels;
using Carnitas.Workflow.Stage;
using Microsoft.Extensions.Hosting;

namespace Carnitas.Workflow.Output;

public class WorkflowOutputCapture : BackgroundService, IWorkflowOutputCapture
{
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
            );
        }
    }
    
    private async Task ProcessStageMessages(IStage stage, CancellationToken cancel)
    {
        if (stage.MessageOutput is null) return;
        
        await foreach (var line in stage.MessageOutput.ReadAllAsync(cancel).ConfigureAwait(false))
        {
            
        }
    }
    
    private async Task ProcessStageJson(IStage stage, CancellationToken cancel)
    {
        if (stage.JsonOutput is null) return;
        
        await foreach (var line in stage.JsonOutput.ReadAllAsync(cancel).ConfigureAwait(false))
        {
            
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Process(stoppingToken);
}