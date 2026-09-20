using System.Threading.Channels;
using Carnitas.Workflow.Stage;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Output;

public class WorkflowOutputProcessor
{
    public async Task ProcessMessage(IStage stage, CancellationToken cancel)
    {
        await foreach (var line in stage.MessageOutput.ReadAllAsync(cancel).ConfigureAwait(false))
        {
            
        }
    }
}