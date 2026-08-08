using System.CommandLine;
using Microsoft.Extensions.Logging;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;
using Sarsoo.Terraform.Module;
using Sarsoo.Terraform.Module.Dependency;

namespace Carnitas.CLI.Command;

public class Plan: System.CommandLine.Command
{

    public Plan()
        : base("plan", "Run tf plan")
    {
        SetAction(Run);
    }

    private async Task<int> Run(ParseResult parseResult)
    {
        var loggingFactory = LoggerFactory.Create(c =>
        {
            c.AddConsole();
        });
        
        var command = new Sarsoo.Terraform.Command.Plan("terraform", ".", loggingFactory.CreateLogger<TerraformStreamCommand<FullMessage>>());

        var task = command.Run();

        await foreach (var line in command.Output.ReadAllAsync())
        {
            Console.WriteLine(line);
        }

        await task;

        return 0;
    }
}