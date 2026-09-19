using Carnitas.Workflow.Output;
using Carnitas.Workflow.Stage;

namespace Carnitas.Workflow.Orchestration;

public interface IWorkflowOrchestrator
{
    string Id { get; }
    void WithOutputCapture(IWorkflowOutputCapture outputCapture);
    IWorkflowOrchestrator AddStage(IStage stage);
    Task<IStageResult> RunNextStage(CancellationToken token = default);
    IAsyncEnumerable<IStageResult> RunAll(CancellationToken token = default);
}