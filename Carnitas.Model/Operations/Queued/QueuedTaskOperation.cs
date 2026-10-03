#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace Carnitas.Model.Operations.Queued;

public class QueuedTaskOperation
{
    public string Id { get; set; }

    public string QueuedTaskId { get; set; }
    public QueuedTask QueuedTask { get; set; }

    public OperationKind Kind { get; set; }
    public int Sequence { get; set; }
}
