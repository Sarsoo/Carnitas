using Carnitas.Grpc;
using Carnitas.Options;
using Carnitas.Workflow.Output;
using Microsoft.Extensions.Options;

namespace Carnitas.CLI.Backend;

public class AgentStatusReporter(
    Agent.AgentClient client,
    IOptions<WorkerOptions> workerOptions
): IStatusReporter
{
    public async Task ReportStatus(string operationId, bool success, int exitCode, string? error = null,
        CancellationToken ct = default)
    {
        await client.ReportOperationStatusAsync(new OperationStatusRequest
        {
            OperationId = operationId,
            State = success ? OperationStatusState.OperationSuccess : OperationStatusState.OperationFailure,
            ExitCode = exitCode,
            Error = error ?? string.Empty
        }, cancellationToken: ct).ConfigureAwait(false);
    }

    public async Task<bool> RenewLease(string taskId, CancellationToken ct = default)
    {
        var response = await client.RenewLeaseAsync(new TaskLeaseRequest
        {
            TaskId = taskId,
            WorkerId = workerOptions.Value.Name
        }, cancellationToken: ct).ConfigureAwait(false);

        return response.Renewed;
    }
}
