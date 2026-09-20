using System.Threading.Channels;
using Carnitas.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage;

public class ApplyStage : IStage
{
    private readonly ModuleStageOptions _options;

    public ApplyStage(ModuleStageOptions options, IOptions<TerraformEnvironmentOptions> envOptions, ILogger<TerraformStreamCommand>? logger = null)
    {
        _options = options;
        Command = new Apply(
            envOptions.Value.BinaryPath,
            options.ModuleLocation,
            logger: logger
        );
    }
    
    public Apply Command { get; private set; }
    public ChannelReader<TerraformMessage>? MessageOutput => Command.Output;
    public ChannelReader<string>? JsonOutput => Command.JsonOutput;

    public string Id { get; }
    public string Name => "Apply";
    public bool Retryable => true;
    
    public async Task<IStageResult> Run(CancellationToken ct = default)
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