namespace Carnitas.Job;

public interface IJob
{
    public string Id { get; }
    public Task Execute(CancellationToken token);
}