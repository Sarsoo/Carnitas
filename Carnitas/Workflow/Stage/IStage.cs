namespace Carnitas.Workflow.Stage;

public interface IStage
{
    bool Retryable { get; }
    Task<IStageResult> Run();
}