using Carnitas.CLI.Options;
using Carnitas.Grpc;
using Carnitas.Job;
using Carnitas.Source;
using Carnitas.Workflow.Orchestration;
using Carnitas.Workflow.Output;
using Carnitas.Workflow.Stage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Carnitas.CLI.Operation;

public class OperationReportingJob(
    IWorkflowOrchestrator orchestrator,
    WorkflowRequestResponse request,
    IStatusReporter statusReporter,
    IOptions<BackendOptions> backendOptions,
    ILogger<OperationReportingJob> logger
): IJob
{
    public string Id => request.Id;

    public string? BasePath
    {
        get => orchestrator.BasePath;
        set => orchestrator.BasePath = value;
    }

    public async Task Execute(CancellationToken token)
    {
        using var leaseCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        var leaseTask = RenewLeaseAsync(leaseCts.Token);

        var currentOperationId = request.Operations.FirstOrDefault()?.Id;
        var sourceScoped = orchestrator as ISourceScopedWorkflowOrchestrator;

        try
        {
            await foreach (var result in orchestrator.RunAll(token).ConfigureAwait(false))
            {
                currentOperationId = result.Id;

                var success = result.Status == StageState.Success;

                await statusReporter.ReportStatus(
                    result.Id,
                    success,
                    success ? 0 : 1,
                    success ? null : $"stage {result.Id} failed",
                    gitReference: sourceScoped?.GitReference,
                    commitSha: sourceScoped?.CommitSha,
                    ct: token).ConfigureAwait(false);

                if (!success)
                {
                    // Stop the workflow on the first failed operation; the queue task is failed server side.
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Workflow {id} failed", request.Id);

            if (currentOperationId is not null)
            {
                await statusReporter.ReportStatus(currentOperationId, false, 1, ex.Message,
                        gitReference: sourceScoped?.GitReference,
                        commitSha: sourceScoped?.CommitSha,
                        ct: CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            await leaseCts.CancelAsync().ConfigureAwait(false);

            try
            {
                await leaseTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
        }
    }

    private async Task RenewLeaseAsync(CancellationToken token)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, backendOptions.Value.LeaseRenewSeconds));

        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                var renewed = await statusReporter.RenewLease(request.Id, CancellationToken.None).ConfigureAwait(false);

                if (!renewed)
                {
                    logger.LogWarning("Lease for task {id} could not be renewed, stopping renewal", request.Id);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to renew lease for task {id}", request.Id);
            }
        }
    }
}
