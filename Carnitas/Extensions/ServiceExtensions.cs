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
                .AddSingleton<TerraformStageOutputCapture>()
                .AddSingleton<ITerraformStageOutputCapture, TerraformStageOutputCapture>(sp => sp.GetRequiredService<TerraformStageOutputCapture>())
                .AddHostedService<TerraformStageOutputCapture>(sp => sp.GetRequiredService<TerraformStageOutputCapture>())
                
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
                .AddTransient<InitTerraformStage>()
                .AddTransient<IStage, InitTerraformStage>()
                
                .AddTransient<PlanTerraformStage>()
                .AddTransient<IStage, PlanTerraformStage>()
                
                .AddTransient<ApplyTerraformStage>()
                .AddTransient<IStage, ApplyTerraformStage>()

                .AddTransient<SourceDiscoveryTerraformStage>()
                .AddTransient<IStage, SourceDiscoveryTerraformStage>();
            
            return serviceCollection;
        }
    }
}