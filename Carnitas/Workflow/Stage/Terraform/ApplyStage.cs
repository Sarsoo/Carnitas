using System.Threading.Channels;
using Carnitas.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage.Terraform;

public class ApplyTerraformStage(
    IOptions<TerraformEnvironmentOptions> envOptions,
    ILogger<TerraformStreamCommand>? logger = null
)
    : TerraformStageBuilder<ApplyTerraformStage>, IStage
{
    public Apply Command { get; private set; }
    public override ChannelReader<TerraformMessage>? MessageOutput => Command.Output;
    public override ChannelReader<string>? JsonOutput => Command.JsonOutput;

    protected override void CreateCommand()
    {
        Command = new Apply(
            envOptions.Value.BinaryPath,
            FullWorkingDirectory,
            logger: logger,
            outputFormat: OutputFormat.Json | OutputFormat.Parsed
        );
    }

    public override string Name => "Apply";
    public override bool Retryable => true;
    
    public override async Task<IStageResult> Run(CancellationToken ct = default)
    {
        try
        {
            await Command.Run(ct).ConfigureAwait(false);
            return Command.Errored ? new StageResult(Id, StageState.Failure) : new StageResult(Id, StageState.Success);
        }
        catch (Exception)
        {
            return new StageResult(Id, StageState.Failure);
        }
    }
}