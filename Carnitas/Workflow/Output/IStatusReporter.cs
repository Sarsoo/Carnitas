namespace Carnitas.Workflow.Output;

public interface IStatusReporter
{
    Task ReportStatus(string operationId, bool success, int exitCode, string? error = null,
        CancellationToken ct = default);

    Task<bool> RenewLease(string taskId, CancellationToken ct = default);
}
