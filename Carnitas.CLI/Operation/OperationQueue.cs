using System.Threading.Channels;
using Carnitas.Grpc;

namespace Carnitas.CLI.Operation;

public class OperationQueue
{
    private readonly Channel<OperationRequestResponse> _operations = Channel.CreateUnbounded<OperationRequestResponse>();
    
    public ChannelReader<OperationRequestResponse> Reader => _operations.Reader;

    public ValueTask AddAsync(OperationRequestResponse response)
    {
        return _operations.Writer.WriteAsync(response);
    }
}