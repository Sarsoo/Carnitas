namespace Carnitas.Options;

public class WorkerOptions
{
    public const string Key = "Worker";
    
    public string Name { get; set; }
    public int ConcurrentJobs { get; set; } = 5;
    public string WorkspaceRoot { get; set; } = "/tmp/cns";
    public string PlanStorageRoot { get; set; } = "/tmp/cns-plan";
}