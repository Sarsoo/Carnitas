namespace Carnitas.Model.Operations;

public interface ITaskQueue
{
    Task<QueuedTask> EnqueueAsync(EnqueueTaskRequest request, CancellationToken ct = default);

    Task<QueuedTask?> ClaimNextAsync(string workerId, TimeSpan lockDuration, CancellationToken ct = default);

    Task<bool> RenewLeaseAsync(string taskId, string workerId, TimeSpan lockDuration, CancellationToken ct = default);

    Task ReportOperationStatusAsync(string operationId, bool success, int? exitCode, string? error,
        CancellationToken ct = default);

    Task AppendLogsAsync(IReadOnlyList<LogLine> entries, CancellationToken ct = default);

    Task SubmitPlanAsync(string operationId, string planJson, string planFilePath, CancellationToken ct = default);

    Task<int> SweepExpiredAsync(CancellationToken ct = default);
}
