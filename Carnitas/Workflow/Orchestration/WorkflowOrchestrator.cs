using System.Runtime.CompilerServices;
using Carnitas.Workflow.Output;
using Carnitas.Workflow.Stage;
using Microsoft.Extensions.Logging;

namespace Carnitas.Workflow.Orchestration;

public class WorkflowOrchestrator(
    IWorkflowOutputCapture workflowOutputCapture, 
    ILogger<WorkflowOrchestrator> logger
): IWorkflowOrchestrator, IDisposable
{
    private Queue<IStage> _stages = new();
    private Queue<IStage> _completedStages = new();
    private bool _disposed = false;
    public bool IsDisposed => _disposed;

    public string Id
    {
        get
        {
            if (field is null)
            {
                throw new InvalidOperationException("Id for the orchestrator has not been set");
            }

            return field;
        }
        private set;
    }

    private IWorkflowOutputCapture _outputCapture = workflowOutputCapture;
    
    public IWorkflowOrchestrator WithOutputCapture(IWorkflowOutputCapture outputCapture)
    {
        _outputCapture = outputCapture;

        return this;
    }
    
    public IWorkflowOrchestrator WithId(string id)
    {
        if (Id is not null)
        {
            throw new  InvalidOperationException("Id for workflow has already been set");
        }
        Id = id;

        return this;
    }

    public IWorkflowOrchestrator AddStage(IStage stage)
    {
        _stages.Enqueue(stage);

        return this;
    }
    
    public async Task<IStageResult> RunNextStage(CancellationToken token = default)
    {
        if (Id is null) throw new InvalidOperationException("Id for the orchestrator has not been set");
        
        var nextStage = _stages.Peek();
        try
        {
            logger.LogInformation("Starting stage {stage}", nextStage.Name);
            
            await _outputCapture.AddStage(nextStage).ConfigureAwait(false);
            
            var result = await nextStage.Run(token).ConfigureAwait(false);

            HandleWorkflowResult(nextStage, result: result);
            return result;
        }
        catch (Exception ex)
        {
            HandleWorkflowResult(nextStage, e: ex);
            return new StageResult(nextStage.Id, StageState.Failure);
        }
    }

    public async IAsyncEnumerable<IStageResult> RunAll([EnumeratorCancellation] CancellationToken token = default)
    {
        if (Id is null) throw new InvalidOperationException("Id for the orchestrator has not been set");
        
        while (_stages.Count > 0 && !_disposed && !token.IsCancellationRequested)
        {
            var result = await RunNextStage(token).ConfigureAwait(false);
            yield return result;
        }
    }

    private void HandleWorkflowResult(IStage stage, IStageResult? result = null, Exception? e = null)
    {
        if (result is { Status: StageState.Failure })
        {
            if (!stage.Retryable)
            {
                Dispose();
                logger.LogError(e, "Workflow failed, non-retryable stage has tainted the workflow which is now disposed");
            }
            else
            {
                logger.LogError(e, "Stage failed");
            }
        }
        else
        {
            _completedStages.Enqueue(_stages.Dequeue());
        }
    }

    public void Dispose()
    {
        _disposed = true;
    }

    public async Task Execute(CancellationToken token)
    {
        await RunAll(token).ToListAsync(token).ConfigureAwait(false);
    }
}