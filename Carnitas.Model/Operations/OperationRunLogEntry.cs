using System.Text.Json;

namespace Carnitas.Model.Operations;

public class OperationRunLogEntry
{
    public string Id { get; set; }
    public string OperationRunId { get; set; }
    public OperationRun OperationRun { get; set; }

    public int Sequence { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Type { get; set; }

    public JsonElement Payload { get; set; }
}
