using Microsoft.Extensions.Logging;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Workflow.Stage;

public class PlanStage : IStage
{
    private readonly ModuleStageOptions _options;

    public PlanStage(ModuleStageOptions options, ILogger<TerraformStreamCommand<FullMessage>>? logger = null)
    {
        _options = options;
        Command = new Plan(
            options.ExePath,
            options.ModuleLocation,
            logger: logger
        );
    }
    
    public Plan Command { get; private set; }

    public bool Retryable => true;
    
    public async Task<IStageResult> Run()
    {
        try
        {
            await Command.Run();
            return new StageResult(StageState.Success);
        }
        catch (Exception)
        {
            return new StageResult(StageState.Failure);
        }
    }
}