namespace Carnitas.Workflow.Stage;

public interface IStageResult
{
    string Id { get; }
    StageState Status { get; }
}