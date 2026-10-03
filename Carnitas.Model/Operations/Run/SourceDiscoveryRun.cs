#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Model.Operations.Run;

public class SourceDiscoveryRun: OperationRun
{
    public string? RepositoryId { get; set; }
    public Repository? Repository { get; set; }
}
