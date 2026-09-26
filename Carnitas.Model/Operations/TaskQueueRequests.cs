namespace Carnitas.Model.Operations;

public record EnqueueTaskRequest(
    string RepoUrl,
    string ModulePath,
    string? ModuleId,
    IReadOnlyList<OperationKind> Operations,
    int Priority = 0,
    int? MaxAttempts = null,
    DateTime? ScheduledAt = null,
    InitiatorType InitiatorType = InitiatorType.System,
    string? InitiatorUserId = null,
    string? RepositoryId = null,
    string? GitReference = null);

public record LogLine(string OperationId, string Log, string Level, string Type);
