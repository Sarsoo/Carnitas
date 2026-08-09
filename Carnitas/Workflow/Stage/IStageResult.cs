namespace Carnitas.Workflow.Stage;

public interface IStageResult
{
    StageState Status { get; }
}