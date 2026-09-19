using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage;

public class ApplyStage : IStage<FullMessage>
{
    private readonly ModuleStageOptions _options;

    public ApplyStage(ModuleStageOptions options, ILogger<TerraformStreamCommand<FullMessage>>? logger = null)
    {
        _options = options;
        Command = new Apply(
            options.ExePath,
            options.ModuleLocation,
            logger: logger
        );
    }
    
    public Apply Command { get; private set; }
    public ChannelReader<FullMessage> Output => Command.Output;

    public string Name => "Apply";
    public bool Retryable => true;
    
    public async Task<IStageResult> Run(CancellationToken ct = default)
    {
        try
        {
            await Command.Run(ct);
            return new StageResult(StageState.Success);
        }
        catch (Exception)
        {
            return new StageResult(StageState.Failure);
        }
    }
}