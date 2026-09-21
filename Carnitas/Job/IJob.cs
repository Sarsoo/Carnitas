namespace Carnitas.Job;

public interface IJob
{
    public string Id { get; }
    public string? BasePath { get; set; }
    public Task Execute(CancellationToken token);
}