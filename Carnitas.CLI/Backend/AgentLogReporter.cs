using Carnitas.Grpc;
using Carnitas.Workflow.Output;

namespace Carnitas.CLI.Backend;

public class AgentLogReporter(Agent.AgentClient client): ILogReporter
{
    public async Task ReportLogs(IReadOnlyList<StageLogLine> lines, CancellationToken ct = default)
    {
        if (lines.Count == 0)
        {
            return;
        }

        var batch = new OperationLogBatch();

        batch.Logs.AddRange(lines.Select(line => new OperationLog
        {
            OperationId = line.OperationId,
            Log = line.Log,
            Level = line.Level,
            Type = line.Type
        }));

        await client.ReportOperationLogBatchAsync(batch, cancellationToken: ct).ConfigureAwait(false);
    }
}
