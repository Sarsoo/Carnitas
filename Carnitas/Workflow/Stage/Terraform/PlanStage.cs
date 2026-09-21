using System.Threading.Channels;
using Carnitas.Options;
using Carnitas.Workflow.Output;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;
using Sarsoo.Terraform.Plan;

namespace Carnitas.Workflow.Stage.Terraform;

public class PlanStage(
    IPlanReporter? planReporter,
    IOptions<TerraformEnvironmentOptions> envOptions,
    ILogger<PlanGenerator> logger,
    ILogger<TerraformStreamCommand> subLogger
)
    : StageBuilder<PlanStage>, IStage
{
    public PlanGenerator Command { get; private set; }
    public override ChannelReader<TerraformMessage>? MessageOutput => Command.PlanOutput;
    public override ChannelReader<string>? JsonOutput => Command.PlanJsonOutput;
    
    public override string Name => "Plan";
    public override bool Retryable => true;
    
    public override async Task<IStageResult> Run(CancellationToken ct = default)
    {
        Command = new PlanGenerator(
            envOptions.Value.BinaryPath,
            _workingDirectory,
            planFileName: "",
            outputFormat: OutputFormat.Parsed | OutputFormat.Json,
            logger: logger,
            subLogger: subLogger
        );
        
        try
        {
            var planJson = await Command.Run(ct).ConfigureAwait(false);

            if (planReporter is not null)
            {
                logger.LogInformation("Plan generated, submitting json result");
                await planReporter.ReportPlan(Id, "", planJson).ConfigureAwait(false);
            }
            
            return new StageResult(Id, StageState.Success);
        }
        catch (Exception)
        {
            return new StageResult(Id, StageState.Failure);
        }
    }
}