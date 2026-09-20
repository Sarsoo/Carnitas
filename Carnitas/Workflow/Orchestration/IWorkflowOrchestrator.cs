using Carnitas.Job;
using Carnitas.Workflow.Output;
using Carnitas.Workflow.Stage;

namespace Carnitas.Workflow.Orchestration;

public interface IWorkflowOrchestrator: IJob
{
    string Id { get; }
    
    IWorkflowOrchestrator WithId(string id);
    IWorkflowOrchestrator WithOutputCapture(IWorkflowOutputCapture outputCapture);
    IWorkflowOrchestrator AddStage(IStage stage);
    Task<IStageResult> RunNextStage(CancellationToken token = default);
    IAsyncEnumerable<IStageResult> RunAll(CancellationToken token = default);
}