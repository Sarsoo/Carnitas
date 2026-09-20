namespace Carnitas.CLI.Options;

public class BackendOptions
{
    public const string Key = "Backend";
    
    public string Url { get; set; }
    public int WorkPollDelay { get; set; } = 5;
}