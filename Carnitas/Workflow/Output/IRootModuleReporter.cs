namespace Carnitas.Workflow.Output;

public interface IRootModuleReporter
{
    Task ReportRootModules(string operationId, string repositoryId, IReadOnlyList<string> relativePaths,
        CancellationToken ct = default);
}
