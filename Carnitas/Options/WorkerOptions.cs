namespace Carnitas.Options;

public class WorkerOptions
{
    public const string Key = "Worker";
    
    public string Name { get; set; }
    public int ConcurrentJobs { get; set; } = 5;
}