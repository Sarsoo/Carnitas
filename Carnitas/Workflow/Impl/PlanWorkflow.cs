using Carnitas.Workflow.Orchestration;
using Carnitas.Workflow.Stage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Impl;

public static class PlanWorkflow
{
    public static IWorkflowOrchestrator Get(string id, ModuleStageOptions options, IServiceProvider sp)
    {
        var orchestrator = new WorkflowOrchestrator(id, sp.GetRequiredService<ILogger<WorkflowOrchestrator>>());

        orchestrator.AddStage(
            new PlanStage(
                options, 
                logger: sp.GetRequiredService<ILogger<TerraformStreamCommand<FullMessage>>>()
            )
        );
        return orchestrator;
    }
}