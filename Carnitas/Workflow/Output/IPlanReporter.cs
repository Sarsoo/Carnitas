namespace Carnitas.Workflow.Output;

public interface IPlanReporter
{
    Task ReportPlan(string planOperationId, string planFileLocation, string plan);
}