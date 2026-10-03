#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace Carnitas.Model.Operations.Queued;

public enum QueuedTaskState
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
