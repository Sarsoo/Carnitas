using System.Threading.Channels;
using Carnitas.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage.Terraform;

public class ApplyStage(
    IOptions<TerraformEnvironmentOptions> envOptions,
    ILogger<TerraformStreamCommand>? logger = null
)
    : StageBuilder<ApplyStage>, IStage
{
    public Apply Command { get; private set; }
    public override ChannelReader<TerraformMessage>? MessageOutput => Command.Output;
    public override ChannelReader<string>? JsonOutput => Command.JsonOutput;

    protected override void CreateCommand()
    {
        Command = new Apply(
            envOptions.Value.BinaryPath,
            FullWorkingDirectory,
            logger: logger
        );
    }

    public override string Name => "Apply";
    public override bool Retryable => true;
    
    public override async Task<IStageResult> Run(CancellationToken ct = default)
    {
        try
        {
            await Command.Run(ct).ConfigureAwait(false);
            return new StageResult(Id, StageState.Success);
        }
        catch (Exception)
        {
            return new StageResult(Id, StageState.Failure);
        }
    }
}