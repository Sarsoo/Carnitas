using Carnitas.Options;
using Carnitas.Workflow.Orchestration;
using Carnitas.Workflow.Stage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carnitas.Extensions;

public static class ServiceExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddCarnitas(IConfiguration configuration)
        {
            serviceCollection.Configure<TerraformEnvironmentOptions>(configuration.GetSection(TerraformEnvironmentOptions.Key));
            
            return serviceCollection;
        }
        
        public IServiceCollection AddWorkflow()
        {
            serviceCollection.AddTransient<IWorkflowOrchestrator, WorkflowOrchestrator>();
            
            return serviceCollection;
        }
        
        public IServiceCollection AddWorkflowStages()
        {
            serviceCollection.AddTransient<PlanStage>()
                .AddTransient<IStage, PlanStage>();
            
            return serviceCollection;
        }
    }
}