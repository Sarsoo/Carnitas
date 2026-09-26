using Carnitas.Job;

namespace Carnitas.Workflow.Stage.Terraform;

public abstract class StageBuilder<TSelf>: IJob, IStage
    where TSelf : StageBuilder<TSelf>

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
    
    protected string FullWorkingDirectory => !string.IsNullOrWhiteSpace(_workingDirectory) ? Path.Join(BasePath, _workingDirectory) : BasePath;

    public async Task Execute(CancellationToken token)
    {
        await Run(token);
    }

    protected abstract void CreateCommand();
    
    public abstract string Name { get; }
    public abstract bool Retryable { get; }
    public abstract Task<IStageResult> Run(CancellationToken ct = default);
}