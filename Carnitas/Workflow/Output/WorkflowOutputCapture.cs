using System.Threading.Channels;
using Carnitas.Workflow.Stage;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Output;

public class WorkflowOutputCapture : IWorkflowOutputCapture
{
    private readonly Channel<IStage> _stageQueue = Channel.CreateUnbounded<IStage>();

    public ValueTask AddStage(IStage stage)
    {
        return _stageQueue.Writer.WriteAsync(stage);
    }

    public async Task Process(CancellationToken cancel = default)
    {
        await foreach (var stage in _stageQueue.Reader.ReadAllAsync(cancel))
        {
            if (stage is IStage<FullMessage> fms)
            {
                await ProcessFullMessageStage(fms, cancel);
            }
        }
    }
    
    private async Task ProcessFullMessageStage(IStage<FullMessage> stage, CancellationToken cancel)
    {
        await foreach (var line in stage.Output.ReadAllAsync(cancel))
        {
            
        }
    }
}