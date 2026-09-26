using Carnitas.CLI.Options;
using Carnitas.Grpc;
using Carnitas.Job;
using Carnitas.Options;
using Carnitas.Source;
using Carnitas.Workflow.Impl;
using Carnitas.Workflow.Output;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Carnitas.CLI.Operation;

public class OperationDispatcher(
    OperationQueue queue,
    IJobDispatcher jobDispatcher,
    IServiceProvider sp,
    IOptions<WorkerOptions> options,
    ILogger<OperationDispatcher> logger
): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting queue processing of received operations..");
        await foreach (var work in queue.Reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var logScope = logger.BeginScope(
                    new[]
                    {
                        new KeyValuePair<string, object>("id", work.Id),
                        new KeyValuePair<string, object>("repo", work.RepoUrl),
                        new KeyValuePair<string, object>("module", work.ModulePath)
                    });
                logger.LogInformation("Processing operation");

                if (work.Operations.Count == 0)
                {
                    logger.LogWarning("Received empty workflow {id}, skipping", work.Id);
                    continue;
                }

                var maxOp = work.Operations.Select(o => o.Operation).Max();

                if (maxOp is OperationType.OperationPlan)
                {
                    logger.LogInformation("Max operation is Plan, generating a plan workflow");
                    var workflow = sp.GetPlanWorkflow<ISourceScopedWorkflowOrchestrator>(
                        work.ModulePath,
                        work.Id,
                        work.Operations.First(o => o.Operation is OperationType.OperationInit).Id,
                        work.Operations.First(o => o.Operation is OperationType.OperationPlan).Id
                    )
                    .WithSourceRoot(options.Value.WorkspaceRoot)
                    .WithSourceUrl(work.RepoUrl)
                    .WithGitReference(work.GitReference)
                    .WithId(work.Id);

                    logger.LogInformation("Queueing plan workflow");
                    jobDispatcher.QueueJob(Wrap(workflow, work));
                }
                else if (maxOp is OperationType.OperationApply)
                {
                    logger.LogInformation("Max operation is Apply, generating an apply workflow");
                    var workflow = sp.GetApplyWorkflow<ISourceScopedWorkflowOrchestrator>(
                            work.ModulePath,
                            work.Id,
                            work.Operations.First(o => o.Operation is OperationType.OperationInit).Id,
                            work.Operations.First(o => o.Operation is OperationType.OperationPlan).Id,
                            work.Operations.First(o => o.Operation is OperationType.OperationApply).Id
                        )
                        .WithSourceRoot(options.Value.WorkspaceRoot)
                        .WithSourceUrl(work.RepoUrl)
                        .WithGitReference(work.GitReference)
                        .WithId(work.Id);

                    logger.LogInformation("Queueing apply workflow");
                    jobDispatcher.QueueJob(Wrap(workflow, work));
                }
                else if (maxOp is OperationType.OperationDiscoverSource)
                {
                    logger.LogInformation("Max operation is DiscoverSource, generating a source discovery workflow");
                    var workflow = sp.GetSourceDiscoveryWorkflow<ISourceScopedWorkflowOrchestrator>(
                            work.Id,
                            work.Operations.First(o => o.Operation is OperationType.OperationDiscoverSource).Id,
                            work.RepositoryId
                        )
                        .WithSourceRoot(options.Value.WorkspaceRoot)
                        .WithSourceUrl(work.RepoUrl)
                        .WithId(work.Id);

                    logger.LogInformation("Queueing source discovery workflow");
                    jobDispatcher.QueueJob(Wrap(workflow, work));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occured while executing work");
            }
        }
    }

    private OperationReportingJob Wrap(Carnitas.Workflow.Orchestration.IWorkflowOrchestrator orchestrator, WorkflowRequestResponse work)
    {
        return new OperationReportingJob(
            orchestrator,
            work,
            sp.GetRequiredService<IStatusReporter>(),
            sp.GetRequiredService<IOptions<BackendOptions>>(),
            sp.GetRequiredService<ILogger<OperationReportingJob>>());
    }
}