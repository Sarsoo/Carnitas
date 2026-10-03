#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace Carnitas.Model.Source.SourceControl;

public class Checkout
{
    public string Id { get; set; }
    public string Path { get; set; }
    public string? Branch { get; set; }
    public string? Commit { get; set; }
    public DateTime CreatedAt { get; set; }

    public string RepositoryId { get; set; }
    public Repository Repository { get; set; }
}
