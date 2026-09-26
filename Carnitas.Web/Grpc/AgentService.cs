using Carnitas.Grpc;
using Carnitas.Model.Operations;
using Grpc.Core;

namespace Carnitas.Web.Grpc;

public class AgentService(ITaskQueue taskQueue, TaskQueueOptions options): Agent.AgentBase
{
    private TimeSpan LockDuration => TimeSpan.FromSeconds(options.LockDurationSeconds);

    public override async Task<OperationLogResponse> ReportOperationLogs(
        IAsyncStreamReader<OperationLog> requestStream, ServerCallContext context)
    {
        var entries = await ReadLogsAsync(requestStream, context.CancellationToken);

        await taskQueue.AppendLogsAsync(entries, context.CancellationToken);

        return new OperationLogResponse();
    }

    public override async Task<OperationLogBatchResponse> ReportOperationLogBatch(
        OperationLogBatch request, ServerCallContext context)
    {
        var entries = request.Logs
            .Select(MapLog)
            .ToList();

        await taskQueue.AppendLogsAsync(entries, context.CancellationToken);

        return new OperationLogBatchResponse();
    }

    public override async Task<OperationStatusResponse> ReportOperationStatus(
        OperationStatusRequest request, ServerCallContext context)
    {
        var success = request.State == OperationStatusState.OperationSuccess;
        var exitCode = request.ExitCode;

        await taskQueue.ReportOperationStatusAsync(
            request.OperationId,
            success,
            exitCode,
            request.Error,
            context.CancellationToken);

        return new OperationStatusResponse
        {
            OperationId = request.OperationId
        };
    }

    public override async Task<OperationPlanResponse> SubmitOperationPlan(
        OperationPlanReport request, ServerCallContext context)
    {
        await taskQueue.SubmitPlanAsync(
            request.OperationId,
            request.Plan,
            request.PlanFilePath,
            context.CancellationToken);

        return new OperationPlanResponse();
    }

    public override async Task<TaskLeaseResponse> RenewLease(TaskLeaseRequest request, ServerCallContext context)
    {
        var renewed = await taskQueue.RenewLeaseAsync(
            request.TaskId,
            request.WorkerId,
            LockDuration,
            context.CancellationToken);

        return new TaskLeaseResponse
        {
            Renewed = renewed
        };
    }

    public override async Task<WorkflowRequestResponse> RequestWorkflow(
        OperationRequest request, ServerCallContext context)
    {
        var task = await taskQueue.ClaimNextAsync(request.WorkerId, LockDuration, context.CancellationToken);

        if (task is null)
        {
            return new WorkflowRequestResponse
            {
                HasWork = false
            };
        }

        var response = new WorkflowRequestResponse
        {
            Id = task.Id,
            RepoUrl = task.RepoUrl,
            ModulePath = task.ModulePath,
            HasWork = true
        };

        foreach (var operation in task.Operations.OrderBy(o => o.Sequence))
        {
            response.Operations.Add(new OperationResponse
            {
                Id = operation.Id,
                Operation = (OperationType) (int) operation.Kind
            });
        }

        return response;
    }

    private static async Task<List<LogLine>> ReadLogsAsync(
        IAsyncStreamReader<OperationLog> requestStream, CancellationToken ct)
    {
        var entries = new List<LogLine>();

        while (await requestStream.MoveNext(ct))
        {
            entries.Add(MapLog(requestStream.Current));
        }

        return entries;
    }

    private static LogLine MapLog(OperationLog log) => new(
        log.OperationId,
        log.Log,
        string.IsNullOrWhiteSpace(log.Level) ? "Information" : log.Level,
        string.IsNullOrWhiteSpace(log.Type) ? "Output" : log.Type);
}
