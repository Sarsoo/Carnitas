using Carnitas.Model.Governance;
using Carnitas.Model.Operations;
using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Model.Source;

public class Module
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string RepositoryId { get; set; }
    public Repository Repository { get; set; }

    public ICollection<OperationRun> OperationRuns { get; }
}