using System.CommandLine;
using Microsoft.Extensions.Logging;
using Sarsoo.Terraform.Command;
using Sarsoo.Terraform.MachineReadableUI;
using Sarsoo.Terraform.Module;
using Sarsoo.Terraform.Module.Dependency;

namespace Carnitas.CLI.Command;

public class Init: System.CommandLine.Command
{

    public Init()
        : base("init", "Run tf init")
    {
        SetAction(Run);
    }

    private async Task<int> Run(ParseResult parseResult)
    {
        var loggingFactory = LoggerFactory.Create(c =>
        {
            c.AddConsole();
        });
        
        var command = new Sarsoo.Terraform.Command.Init("terraform", ".", logger: loggingFactory.CreateLogger<TerraformStreamCommand>());

        var task = command.Run();

        await foreach (var line in command.Output!.ReadAllAsync())
        {
            Console.WriteLine($"{line.GetType()}: {line}");
        }

        await task;

        return 0;
    }
}