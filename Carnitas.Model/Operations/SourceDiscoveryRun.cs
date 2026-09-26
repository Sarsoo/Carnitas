using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Model.Operations;

public class SourceDiscoveryRun: OperationRun
{
    public string? RepositoryId { get; set; }
    public Repository? Repository { get; set; }
}
