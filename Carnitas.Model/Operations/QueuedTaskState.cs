namespace Carnitas.Model.Operations;

public enum QueuedTaskState
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
