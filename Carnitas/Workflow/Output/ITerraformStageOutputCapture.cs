using Carnitas.Workflow.Stage;

namespace Carnitas.Workflow.Output;

public interface ITerraformStageOutputCapture
{
    ValueTask AddStage(ITerraformStage stage);
    Task Process(CancellationToken cancel = default);
}