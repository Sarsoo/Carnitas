using Carnitas.Grpc;
using Carnitas.Workflow.Output;

namespace Carnitas.CLI.Backend;

public class AgentRootModuleReporter(Agent.AgentClient client): IRootModuleReporter
{
    public async Task ReportRootModules(string operationId, string repositoryId,
        IReadOnlyList<string> relativePaths, CancellationToken ct = default)
    {
        var contents = new RootModuleContents
        {
            OperationId = operationId,
            RepositoryId = repositoryId
        };

        contents.ModulePaths.AddRange(relativePaths);

        await client.RecordRootModulesAsync(contents, cancellationToken: ct).ConfigureAwait(false);
    }
}
