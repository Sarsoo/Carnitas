using System.CommandLine;
using Carnitas.CLI.Command;


Root rootCommand = new();
return await rootCommand.Parse(args).InvokeAsync();