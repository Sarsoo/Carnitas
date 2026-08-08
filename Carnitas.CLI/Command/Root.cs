using System.CommandLine;

namespace Carnitas.CLI.Command;

public class Root: RootCommand
{
    public Root():
        base("Carnitas - a TACO")
    {
        Add(new Dependents());
        Add(new Dependencies());
        Add(new Plan());
    }
}