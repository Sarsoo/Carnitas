using Carnitas.Grpc;
using Grpc.Core;

namespace Carnitas.Web.Grpc;

public class AgentService: Agent.AgentBase
{
    public override Task<OperationLogResponse> ReportOperationLogs(IAsyncStreamReader<OperationLog> requestStream, ServerCallContext context)
    {
        return base.ReportOperationLogs(requestStream, context);
    }

    private static int count = 0;

    public override async Task<WorkflowRequestResponse> RequestWorkflow(OperationRequest request, ServerCallContext context)
    {
        if (count < 1)
        {
            var resp = new WorkflowRequestResponse()
            {
                Id = Guid.NewGuid().ToString(),
                RepoUrl = "git@github.com:Sarsoo/infra.git",
                ModulePath = "/terraform/PROXMOX/biggie"
            };
        
            resp.Operations.Add(new OperationResponse
            {
                Id = Guid.NewGuid().ToString(),
                Operation = OperationType.OperationInit
            });
            resp.Operations.Add(new OperationResponse
            {
                Id = Guid.NewGuid().ToString(),
                Operation = OperationType.OperationPlan
            });
            count++;
            return resp;

        }

        return null;
    }
}