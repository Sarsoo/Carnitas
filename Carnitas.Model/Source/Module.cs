using Carnitas.Model.Governance;
using Carnitas.Model.Operations;
using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Model.Source;

public class Module
{
    public string Id { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Repository-relative path to the module directory. Part of the logical identity of the
    /// module (alongside <see cref="RepositoryId"/>), independent of branch.
    /// </summary>
    public string? RelativePath { get; set; }

    public string RepositoryId { get; set; }
    public Repository Repository { get; set; }

    public ICollection<OperationRun> OperationRuns { get; }
}