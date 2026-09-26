using System.Threading.Channels;
using Carnitas.Job;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage.Terraform;

public abstract class TerraformStageBuilder<TSelf>: IJob, ITerraformStage
    where TSelf : TerraformStageBuilder<TSelf>

{
    public string Id { get; private set; }

    public string? BasePath
    {
        get;
        set
        {
            field = value;
            if (!string.IsNullOrEmpty(_workingDirectory) && !string.IsNullOrWhiteSpace(BasePath))
            {
                CreateCommand();
            }
        }
    }

    protected string? _workingDirectory = null;

    public TSelf WithId(string id)
    {
        Id = id;
        return (TSelf) this;
    }
    
    public TSelf WithWorkingDirectory(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
        if (!string.IsNullOrEmpty(_workingDirectory) && !string.IsNullOrWhiteSpace(BasePath))
        {
            CreateCommand();
        }
        return (TSelf) this;
    }
    
    protected string FullWorkingDirectory => Path.Join(BasePath, _workingDirectory);

    public async Task Execute(CancellationToken token)
    {
        await Run(token);
    }

    protected abstract void CreateCommand();
    
    public abstract string Name { get; }
    public abstract bool Retryable { get; }
    public abstract Task<IStageResult> Run(CancellationToken ct = default);
    public abstract ChannelReader<TerraformMessage>? MessageOutput { get; }
    public abstract ChannelReader<string>? JsonOutput { get; }
}