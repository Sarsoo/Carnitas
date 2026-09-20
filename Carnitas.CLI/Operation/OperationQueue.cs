using System.Threading.Channels;
using Carnitas.Grpc;

namespace Carnitas.CLI.Operation;

public class OperationQueue
{
    private readonly Channel<WorkflowRequestResponse> _operations = Channel.CreateUnbounded<WorkflowRequestResponse>();
    
    public ChannelReader<WorkflowRequestResponse> Reader => _operations.Reader;

    public ValueTask AddAsync(WorkflowRequestResponse response)
    {
        return _operations.Writer.WriteAsync(response);
    }
}