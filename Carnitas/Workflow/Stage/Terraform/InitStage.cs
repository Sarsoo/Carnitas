using System.Threading.Channels;
using Carnitas.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage.Terraform;

public class InitStage(
    IOptions<TerraformEnvironmentOptions> envOptions,
    ILogger<TerraformStreamCommand>? logger = null
)
    : StageBuilder<InitStage>, IStage
{
    public Init Command { get; private set; }
    public override ChannelReader<TerraformMessage>? MessageOutput => Command.Output;
    public override ChannelReader<string>? JsonOutput => Command.JsonOutput;
    
    public override string Name => "Init";
    public override bool Retryable => true;
    
    public override async Task<IStageResult> Run(CancellationToken ct = default)
    {
        Command = new Init(
            envOptions.Value.BinaryPath,
            _workingDirectory,
            logger: logger
        );
        
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