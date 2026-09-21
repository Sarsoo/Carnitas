using Carnitas.Workflow.Orchestration;
using Carnitas.Workflow.Stage.Terraform;
using Microsoft.Extensions.DependencyInjection;

namespace Carnitas.Workflow.Impl;

public static class WorkflowExtensions
{
    extension(IServiceProvider sp)
    {
        public IWorkflowOrchestrator GetPlanWorkflow(string moduleDirectory, string workflowId, string initId,
            string planId)
            => sp.GetPlanWorkflow<IWorkflowOrchestrator>(moduleDirectory,  workflowId, initId, planId);
        
        public IWorkflowOrchestrator GetPlanWorkflow<T>(string moduleDirectory, string workflowId, string initId, string planId) where T: IWorkflowOrchestrator
        {
            var orchestrator = sp.GetRequiredService<T>();
        
            var init = sp.GetRequiredService<InitStage>()
                .WithWorkingDirectory(moduleDirectory)
                .WithId(initId);
            var plan = sp.GetRequiredService<PlanStage>()
                .WithWorkingDirectory(moduleDirectory)
                .WithId(planId);

            orchestrator
                .WithId(workflowId)
                .AddStage(init)
                .AddStage(plan);
        
            return orchestrator;
        }
    }
}