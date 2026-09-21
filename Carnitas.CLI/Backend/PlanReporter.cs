using Carnitas.Grpc;
using Carnitas.Workflow.Output;

namespace Carnitas.CLI.Backend;

public class PlanReporter(Agent.AgentClient client): IPlanReporter
{
    public async Task ReportPlan(string planOperationId, string planFileLocation, string plan)
    {
        await client.SubmitOperationPlanAsync(new OperationPlanReport
        {
            OperationId = planOperationId,
            Plan = plan,
            PlanFilePath = planFileLocation
        });
    }
}