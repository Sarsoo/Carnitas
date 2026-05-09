using System.CommandLine;
using Sarsoo.Terraform.Module;
using Sarsoo.Terraform.Module.Dependency;
using TerraformDotnet.Hcl.Nodes;

namespace Carnitas.CLI.Command;

public class Dependencies: System.CommandLine.Command
{
    private readonly Argument<DirectoryInfo?> _modulePath = new("path")
    {
        Description = "Path to a module for which to identify dependent modules",
        DefaultValueFactory = static (_) => null
    };

    private readonly Option<DirectoryInfo?> _workingDirectory = new("--dir", "-c")
    {
        Description = "Working directory from which to search for dependents (defaults to current directory)",
        DefaultValueFactory = static (_) => null
    };

    private readonly Option<bool> _relative = new("--relative", "-r")
    {
        Description = "Whether to leave local module references with their relative path references",
        DefaultValueFactory = static (_) => false
    };

    public Dependencies()
        : base("dep", "Identify the dependents of a module")
    {
        SetAction(Run);

        Add(_modulePath);
        Add(_workingDirectory);
        Add(_relative);
    }

    private async Task<int> Run(ParseResult parseResult)
    {
        var modPath = parseResult.GetValue(_modulePath);
        var workingDirectory = parseResult.GetValue(_workingDirectory);
        var pipedInput = Console.IsInputRedirected ? Console.ReadLine() : null;

        var dependencyResolver = new DependencyResolver(!parseResult.GetValue(_relative));

        var path = pipedInput ?? workingDirectory?.FullName;

        if (path is null)
        {
            return 1;
        }

        dependencyResolver.Resolve(path);

        foreach (var forward in dependencyResolver.ForwardDependencies)
        {
            foreach (var dep in forward.Value)
            {
                var modSource = dep.Source.SourceReference();

                Console.WriteLine($"{forward.Key}\t{dep.Path}\t{modSource.Version}");
            }
        }

        return 0;
    }
}