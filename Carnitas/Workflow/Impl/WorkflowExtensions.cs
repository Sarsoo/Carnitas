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
        
        public T GetPlanWorkflow<T>(string moduleDirectory, string workflowId, string initId, string planId) where T: IWorkflowOrchestrator
        {
            var orchestrator = sp.GetRequiredService<T>();
        
            var init = sp.GetRequiredService<InitTerraformStage>()
                .WithWorkingDirectory(moduleDirectory)
                .WithId(initId);
            var plan = sp.GetRequiredService<PlanTerraformStage>()
                .WithWorkingDirectory(moduleDirectory)
                .WithId(planId);

            orchestrator
                .WithId(workflowId)
                .AddStage(init)
                .AddStage(plan);
        
            return orchestrator;
        }
        
        public IWorkflowOrchestrator GetApplyWorkflow(string moduleDirectory, string workflowId, string initId,
            string planId)
            => sp.GetPlanWorkflow<IWorkflowOrchestrator>(moduleDirectory,  workflowId, initId, planId);
        
        public T GetApplyWorkflow<T>(string moduleDirectory, string workflowId, string initId, string planId, string applyId) where T: IWorkflowOrchestrator
        {
            var orchestrator = sp.GetRequiredService<T>();
        
            var init = sp.GetRequiredService<InitTerraformStage>()
                .WithWorkingDirectory(moduleDirectory)
                .WithId(initId);
            var plan = sp.GetRequiredService<PlanTerraformStage>()
                .WithWorkingDirectory(moduleDirectory)
                .WithId(planId);
            var apply = sp.GetRequiredService<ApplyTerraformStage>()
                .WithWorkingDirectory(moduleDirectory)
                .WithId(applyId);

            orchestrator
                .WithId(workflowId)
                .AddStage(init)
                .AddStage(plan)
                .AddStage(apply);
        
            return orchestrator;
        }
    }
}