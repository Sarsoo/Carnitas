using System.CommandLine;
using Sarsoo.Terraform.Module;
using Sarsoo.Terraform.Module.Dependency;

namespace Carnitas.CLI.Command;

public class Dependents: System.CommandLine.Command
{
    private readonly Argument<DirectoryInfo?> _modulePath = new("path")
    {
        Description = "Path to a module for which to identify dependencies modules",
        DefaultValueFactory = static (_) => null
    };

    private readonly Option<DirectoryInfo?> _workingDirectory = new("--dir", "-c")
    {
        Description = "Working directory from which to search for dependencies (defaults to current directory)",
        DefaultValueFactory = static (_) => null
    };

    private readonly Option<bool> _relative = new("--relative", "-r")
    {
        Description = "Whether to leave local module references with their relative path references",
        DefaultValueFactory = static (_) => false
    };

    public Dependents()
        : base("rdep", "Identify the dependents of a module")
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

        foreach (var reverse in dependencyResolver.ReverseDependencies)
        {
            foreach (var dep in reverse.Value)
            {
                Console.WriteLine($"{reverse.Key}\t{dep.Path}");
            }
        }

        return 0;
    }
}