using System.Threading.Channels;
using Carnitas.Job;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage.Terraform;

public abstract class StageBuilder<TSelf>: IJob, IStage
    where TSelf : StageBuilder<TSelf>

{
    public string Id { get; private set; }
    protected string? _workingDirectory = null;

    public TSelf WithId(string id)
    {
        Id = id;
        return (TSelf) this;
    }
    
    public TSelf WithWorkingDirectory(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
        return (TSelf) this;
    }

    public async Task Execute(CancellationToken token)
    {
        await Run(token);
    }
    
    public abstract string Name { get; }
    public abstract bool Retryable { get; }
    public abstract Task<IStageResult> Run(CancellationToken ct = default);
    public abstract ChannelReader<TerraformMessage>? MessageOutput { get; }
    public abstract ChannelReader<string>? JsonOutput { get; }
}