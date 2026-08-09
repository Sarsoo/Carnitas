namespace Carnitas.Workflow.Stage;

public record StageResult(StageState Status): IStageResult;