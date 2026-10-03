using System.Threading.Channels;
using Carnitas.Exceptions;
using Carnitas.Observability;
using Carnitas.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage.Terraform;

public class InitTerraformStage(
    IOptions<TerraformEnvironmentOptions> envOptions,
    ILogger<TerraformStreamCommand> logger
)
    : TerraformStageBuilder<InitTerraformStage>, IStage
{
    public Init? Command { get; private set; }
    public override ChannelReader<TerraformMessage>? MessageOutput => Command?.Output;
    public override ChannelReader<string>? JsonOutput => Command?.JsonOutput;
    
    public override bool Errored => Command?.Errored ?? throw new StageNotInitialisedException();
    public override int ExitCode => Command?.ExitCode ?? throw new StageNotInitialisedException();

    protected override void CreateCommand()
    {
        Command = new Init(
            FullWorkingDirectory,
            logger: logger,
            outputFormat: OutputFormat.Json | OutputFormat.Parsed,
            tfExecutable: envOptions.Value.BinaryPath
        );
    }

    public override string Name => "Init";
    public override bool Retryable => true;
    
    public override async Task<IStageResult> Run(CancellationToken ct = default)
    {
        using var trace = Tracing.Source.StartActivity("InitStage::Run");
        Baggage.SetBaggage(ObservabilityConstants.StageId, Id);
        try
        {
            if(Command is null) throw new StageNotInitialisedException();
            
            await Command.Run(ct).ConfigureAwait(false);
            return Command.Errored ? new StageResult(Id, StageState.Failure) : new StageResult(Id, StageState.Success);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occurred while executing InitStage");
            return new StageResult(Id, StageState.Failure);
        }
    }
}