namespace Carnitas.Workflow.Stage;

public record StageResult(string Id, StageState Status): IStageResult;