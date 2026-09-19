using Carnitas.Workflow.Stage;

namespace Carnitas.Workflow.Output;

public interface IWorkflowOutputCapture
{
    ValueTask AddStage(IStage stage);
    Task Process(CancellationToken cancel = default);
}