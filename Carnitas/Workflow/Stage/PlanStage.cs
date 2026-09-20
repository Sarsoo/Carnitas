using System.Threading.Channels;
using Carnitas.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage;

public class PlanStage : IStage
{
    private readonly ModuleStageOptions _options;

    public PlanStage(ModuleStageOptions options, IOptions<TerraformEnvironmentOptions> envOptions, ILogger<TerraformStreamCommand>? logger = null)
    {
        _options = options;
        Command = new Plan(
            envOptions.Value.BinaryPath,
            options.ModuleLocation,
            logger: logger
        );
    }
    
    public Plan Command { get; private set; }
    public ChannelReader<TerraformMessage> Output => Command.Output;

    public string Name => "Plan";
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