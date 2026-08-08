namespace Carnitas.Model.Source;

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
