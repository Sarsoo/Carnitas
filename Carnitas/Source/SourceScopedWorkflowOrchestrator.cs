using Carnitas.Workflow.Orchestration;
using Carnitas.Workflow.Output;
using Carnitas.Workflow.Stage;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace Carnitas.Source;

public interface ISourceScopedWorkflowOrchestrator: IWorkflowOrchestrator
{
    ISourceScopedWorkflowOrchestrator PrepareSource();
    ISourceScopedWorkflowOrchestrator WithSourceUrl(string sourceUrl);
    ISourceScopedWorkflowOrchestrator WithSourceRoot(string sourceRoot);
    ISourceScopedWorkflowOrchestrator WithCloneOptions(CloneOptions options);
}

public class SourceScopedWorkflowOrchestrator(
    IWorkflowOrchestrator workflowOrchestrator,
    ILogger<SourceScopedWorkflowOrchestrator> logger
): ISourceScopedWorkflowOrchestrator, IDisposable
{
    private IWorkflowOrchestrator _workflowOrchestrator = workflowOrchestrator;
    
    private string? _sourceRoot;
    private string? _gitUrl;
    private CloneOptions? _cloneOptions;

    private string? _checkoutPath;
    private bool _disposed = false;
    
    public bool IsDisposed => _disposed;

    private void Validate()
    {
        if (_workflowOrchestrator is null)
        {
            throw new ArgumentException("Child workflow orchestrator has not been provided");
        }
        
        if (string.IsNullOrEmpty(_sourceRoot))
        {
            throw new ArgumentException("Directory root for the source checkout has not been provided");
        }
        
        if (string.IsNullOrEmpty(_gitUrl))
        {
            throw new ArgumentException("Git url has not been provided");
        }
    }

    public string Id => _workflowOrchestrator.Id;

    public string? BasePath
    {
        get;
        set
        {
            field = value;
            _workflowOrchestrator.BasePath = field;
        }
    }

    public IWorkflowOrchestrator WithId(string id)
    {
        _workflowOrchestrator = _workflowOrchestrator.WithId(id);
        return this;
    }

    public IWorkflowOrchestrator WithOutputCapture(ITerraformStageOutputCapture outputCapture)
    {
        _workflowOrchestrator = _workflowOrchestrator.WithOutputCapture(outputCapture);
        return this;
    }

    public IWorkflowOrchestrator AddStage(IStage stage)
    {
        _workflowOrchestrator = _workflowOrchestrator.AddStage(stage);
        return this;
    }

    public Task<IStageResult> RunNextStage(CancellationToken token = default)
    { 
        Validate();
        if (string.IsNullOrEmpty(_checkoutPath))
        {
            throw new InvalidOperationException("Source has not been checked out");
        }
        return _workflowOrchestrator.RunNextStage(token);
    }

    public IAsyncEnumerable<IStageResult> RunAll(CancellationToken token = default)
    {
        Validate();
        if (string.IsNullOrEmpty(_checkoutPath))
        {
            PrepareSource();
        }
        return _workflowOrchestrator.RunAll(token);
    }

    public Task Execute(CancellationToken token)
    {
        Validate();
        if (string.IsNullOrEmpty(_checkoutPath))
        {
            PrepareSource();
        }
        return _workflowOrchestrator.Execute(token);
    }
    
    public void Dispose()
    {
        _disposed = true;
        if (!string.IsNullOrWhiteSpace(_checkoutPath) && Directory.Exists(_checkoutPath))
        {
            logger.LogInformation("Deleting checkout at {Path}", _checkoutPath);
            Directory.Delete(_checkoutPath, true);
        }
    }

    public ISourceScopedWorkflowOrchestrator PrepareSource()
    {
        Validate();

        var checkoutPath = Path.Join(_sourceRoot, Id);
        logger.LogInformation("Checking out source at {Path}", checkoutPath);
        _checkoutPath = Repository.Clone(_gitUrl, checkoutPath, _cloneOptions);
        
        BasePath = checkoutPath;
        
        return this;
    }

    public ISourceScopedWorkflowOrchestrator WithSourceUrl(string sourceUrl)
    {
        _gitUrl = sourceUrl;
        return this;
    }

    public ISourceScopedWorkflowOrchestrator WithSourceRoot(string sourceRoot)
    {
        _sourceRoot = sourceRoot;
        return this;
    }

    public ISourceScopedWorkflowOrchestrator WithSubOrchestrator(IWorkflowOrchestrator workflowOrchestrator)
    {
        _workflowOrchestrator = workflowOrchestrator;
        return this;
    }

    public ISourceScopedWorkflowOrchestrator WithCloneOptions(CloneOptions options)
    {
        _cloneOptions = options;
        return this;
    }
}