namespace Carnitas.Model.Operations;

public class TaskQueueOptions
{
    public const string Key = "TaskQueue";

    public int LockDurationSeconds { get; set; } = 300;
    public int LeaseRenewSeconds { get; set; } = 60;
    public int MaxAttempts { get; set; } = 3;
    public int SweepIntervalSeconds { get; set; } = 60;
}
