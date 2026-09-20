using Carnitas.Grpc;
using Grpc.Core;

namespace Carnitas.Web.Grpc;

public class AgentService: Agent.AgentBase
{
    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PingResponse() { Id = request.Id});
    }

    public override Task<OperationLog> ReportOperationLogs(IAsyncStreamReader<OperationLog> requestStream, ServerCallContext context)
    {
        return base.ReportOperationLogs(requestStream, context);
    }

    public override async Task<OperationRequestResponse> RequestOperation(OperationRequest request, ServerCallContext context)
    {
        return new OperationRequestResponse()
        {
            Id = Guid.NewGuid().ToString(),
            Repo = "test repo",
            ModulePath = "test module",
            Operation = "plan"
        };
    }
}