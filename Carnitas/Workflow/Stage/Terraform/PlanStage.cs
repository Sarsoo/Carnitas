using System.Threading.Channels;
using Carnitas.Exceptions;
using Carnitas.Observability;
using Carnitas.Options;
using Carnitas.Workflow.Output;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;
using Sarsoo.Terraform.Plan;

namespace Carnitas.Workflow.Stage.Terraform;

public class PlanTerraformStage(
    IPlanReporter? planReporter,
    IOptions<WorkerOptions> workerOptions,
    IOptions<TerraformEnvironmentOptions> envOptions,
    ILogger<PlanGenerator> logger,
    ILogger<TerraformStreamCommand> subLogger
)
    : TerraformStageBuilder<PlanTerraformStage>, IStage
{
    public PlanGenerator? Command { get; private set; }
    public override ChannelReader<TerraformMessage>? MessageOutput => Command?.Output;
    public override ChannelReader<string>? JsonOutput => Command?.JsonOutput;
    
    public override bool Errored => Command?.Errored ?? throw new StageNotInitialisedException();
    public override int ExitCode => Command?.ExitCode ?? throw new StageNotInitialisedException();

    private string? planBinPath = null;

    protected override void CreateCommand()
    {
        planBinPath = Path.Join(workerOptions.Value.PlanStorageRoot, $"{Id}.tfplan");
        Command = new PlanGenerator(
            FullWorkingDirectory,
            tfExecutable: envOptions.Value.BinaryPath,
            planFileName: Path.Join(workerOptions.Value.PlanStorageRoot, $"{Id}.tfplan"),
            outputFormat: OutputFormat.Parsed | OutputFormat.Json,
            logger: logger,
            subLogger: subLogger
        );
    }

    public override string Name => "Plan";
    public override bool Retryable => true;
    
    public override async Task<IStageResult> Run(CancellationToken ct = default)
    {
        using var trace = Tracing.Source.StartActivity("PlanStage::Run");
        Baggage.SetBaggage(ObservabilityConstants.StageId, Id);
        try
        {
            if(Command is null) throw new StageNotInitialisedException();
            
            var planJson = await Command.Run(ct).ConfigureAwait(false);

            if (planReporter is not null)
            {
                logger.LogInformation("Plan generated, submitting json result");
                await planReporter.ReportPlan(Id, planBinPath ?? "", planJson).ConfigureAwait(false);
            }
            
            return Command.Errored ? new StageResult(Id, StageState.Failure) : new StageResult(Id, StageState.Success);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occurred while executing PlanStage");
            return new StageResult(Id, StageState.Failure);
        }
    }
}