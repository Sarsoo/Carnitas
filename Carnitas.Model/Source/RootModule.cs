using Carnitas.Model.Governance;
using Carnitas.Model.Operations;

namespace Carnitas.Model.Source;

public class RootModule
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string? TrackingBranch { get; set; }

    public string RepositoryId { get; set; }
    public Repository Repository { get; set; }

    public ICollection<OperationRun> OperationRuns { get; }
}