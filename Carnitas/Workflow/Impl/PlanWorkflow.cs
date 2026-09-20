using Carnitas.Options;
using Carnitas.Workflow.Orchestration;
using Carnitas.Workflow.Stage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Impl;

public static class PlanWorkflow
{
    public static IWorkflowOrchestrator Get(string id, ModuleStageOptions options, IServiceProvider sp)
    {
        var orchestrator = sp.GetRequiredService<IWorkflowOrchestrator>();

        orchestrator
            .WithId(id)
            .AddStage(
                new PlanStage(
                    options, 
                    sp.GetRequiredService<IOptions<TerraformEnvironmentOptions>>(),
                    logger: sp.GetRequiredService<ILogger<TerraformStreamCommand>>()
                )
            );
        return orchestrator;
    }
}