using Carnitas.Job;
using Carnitas.Options;
using Carnitas.Source;
using Carnitas.Workflow.Orchestration;
using Carnitas.Workflow.Output;
using Carnitas.Workflow.Stage;
using Carnitas.Workflow.Stage.Terraform;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sarsoo.Terraform.Command;

namespace Carnitas.Extensions;

public static class ServiceExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddCarnitas(IConfiguration configuration)
        {
            serviceCollection.Configure<TerraformEnvironmentOptions>(configuration.GetSection(TerraformEnvironmentOptions.Key));

            serviceCollection.AddSingleton<IJobDispatcher, JobDispatcher>()
                .AddSingleton<ICheckoutManager, CheckoutManager>()
                .AddWorkflow()
                .AddWorkflowStages();
            
            return serviceCollection;
        }
        
        public IServiceCollection AddWorkflow()
        {
            serviceCollection
                .AddSingleton<WorkflowOutputCapture>()
                .AddSingleton<IWorkflowOutputCapture, WorkflowOutputCapture>(sp => sp.GetRequiredService<WorkflowOutputCapture>())
                .AddHostedService<WorkflowOutputCapture>(sp => sp.GetRequiredService<WorkflowOutputCapture>())
                
                .AddSingleton<JobDispatcher>()
                .AddSingleton<IJobDispatcher, JobDispatcher>(sp => sp.GetRequiredService<JobDispatcher>())
                .AddHostedService<JobDispatcher>(sp => sp.GetRequiredService<JobDispatcher>())
                
                .AddTransient<IWorkflowOrchestrator, WorkflowOrchestrator>()
                .AddTransient<ISourceScopedWorkflowOrchestrator, SourceScopedWorkflowOrchestrator>();
            
            return serviceCollection;
        }
        
        public IServiceCollection AddWorkflowStages()
        {
            serviceCollection
                .AddTransient<InitStage>()
                .AddTransient<IStage, InitStage>()
                
                .AddTransient<PlanStage>()
                .AddTransient<IStage, PlanStage>()
                
                .AddTransient<ApplyStage>()
                .AddTransient<IStage, ApplyStage>();
            
            return serviceCollection;
        }
    }
}