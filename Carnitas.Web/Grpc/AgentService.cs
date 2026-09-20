using Carnitas.Grpc;
using Grpc.Core;

namespace Carnitas.Web.Grpc;

public class AgentService: Agent.AgentBase
{
    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PingResponse() { Id = request.Id});
    }

    public override Task<OperationLogResponse> ReportOperationLogs(IAsyncStreamReader<OperationLog> requestStream, ServerCallContext context)
    {
        return base.ReportOperationLogs(requestStream, context);
    }

    public override async Task<WorkflowRequestResponse> RequestWorkflow(OperationRequest request, ServerCallContext context)
    {
        var resp = new WorkflowRequestResponse()
        {
            Id = Guid.NewGuid().ToString(),
            Repo = "test repo",
            ModulePath = "test module"
        };
        
        resp.Operations.Add(new OperationResponse
        {
            Id = Guid.NewGuid().ToString(),
        });

        return resp;
    }
}