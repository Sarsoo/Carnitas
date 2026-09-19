using System.Threading.Channels;

namespace Carnitas.Workflow.Stage;

public interface IStage
{
    string Name { get; }
    bool Retryable { get; }
    Task<IStageResult> Run(CancellationToken ct = default);
}

public interface IStage<T>: IStage
{
    public ChannelReader<T> Output { get; }
}