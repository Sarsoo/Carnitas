#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace Carnitas.Model.Source.SourceControl.GitHub;

public class GitHubApp
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? InstanceUrl { get; set; }
    public string PrivateKey { get; set; }
}