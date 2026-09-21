using Carnitas.Grpc;
using Carnitas.Job;
using Carnitas.Options;
using Carnitas.Source;
using Carnitas.Workflow.Impl;
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
                    .WithId(work.Id);

                    logger.LogInformation("Queueing plan workflow");
                    jobDispatcher.QueueJob(workflow);
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
                        .WithId(work.Id);

                    logger.LogInformation("Queueing apply workflow");
                    jobDispatcher.QueueJob(workflow);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occured while executing work");
            }
        }
    }
}