using System.Runtime.CompilerServices;
using Carnitas.Workflow.Output;
using Carnitas.Workflow.Stage;
using Microsoft.Extensions.Logging;

namespace Carnitas.Workflow.Orchestration;

public class WorkflowOrchestrator(string id, ILogger<WorkflowOrchestrator> logger): IWorkflowOrchestrator, IDisposable
{
    private Queue<IStage> _stages = new();
    private Queue<IStage> _completedStages = new();
    private bool _disposed = false;
    public bool IsDisposed => _disposed;
    public string Id { get; } = id;

    private IWorkflowOutputCapture? _outputCapture;
    public void WithOutputCapture(IWorkflowOutputCapture outputCapture)
    {
        _outputCapture = outputCapture;
    }

    public IWorkflowOrchestrator AddStage(IStage stage)
    {
        _stages.Enqueue(stage);

        return this;
    }
    
    public async Task<IStageResult> RunNextStage(CancellationToken token = default)
    {
        var nextStage = _stages.Peek();
        try
        {
            logger.LogInformation("Starting stage {stage}", nextStage.Name);
            if (_outputCapture?.AddStage(nextStage) is { } t)
            {
                await t;
            }
            var result = await nextStage.Run(token);

            HandleWorkflowResult(nextStage, result: result);
            return result;
        }
        catch (Exception ex)
        {
            HandleWorkflowResult(nextStage, e: ex);
            return new StageResult(StageState.Failure);
        }
    }

    public async IAsyncEnumerable<IStageResult> RunAll([EnumeratorCancellation] CancellationToken token = default)
    {
        while (_stages.Count > 0 && !_disposed && !token.IsCancellationRequested)
        {
            var result = await RunNextStage(token);
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
}