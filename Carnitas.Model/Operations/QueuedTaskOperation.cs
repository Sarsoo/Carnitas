namespace Carnitas.Model.Operations;

public class QueuedTaskOperation
{
    public string Id { get; set; }

    public string QueuedTaskId { get; set; }
    public QueuedTask QueuedTask { get; set; }

    public OperationKind Kind { get; set; }
    public int Sequence { get; set; }
}
