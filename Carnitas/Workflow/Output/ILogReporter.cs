using Carnitas.Job;

namespace Carnitas.Workflow.Output;

public record StageLogLine(string OperationId, string Log, string Level, LogType Type);

public interface ILogReporter
{
    Task ReportLogs(IReadOnlyList<StageLogLine> lines, CancellationToken ct = default);
}
