using System.Threading.Channels;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage;

public interface IStage
{
    string Name { get; }
    bool Retryable { get; }
    Task<IStageResult> Run(CancellationToken ct = default);
    public ChannelReader<TerraformMessage> Output { get; }
}