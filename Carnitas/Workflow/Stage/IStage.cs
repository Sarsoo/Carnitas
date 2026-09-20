using System.Threading.Channels;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage;

public interface IStage
{
    string Id { get; }
    string Name { get; }
    bool Retryable { get; }
    Task<IStageResult> Run(CancellationToken ct = default);
    public ChannelReader<TerraformMessage>? MessageOutput { get; }
    public ChannelReader<string>? JsonOutput { get; }
}