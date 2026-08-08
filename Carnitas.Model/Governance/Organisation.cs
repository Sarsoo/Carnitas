using Carnitas.Model.Source;
using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Model.Governance;

public class Organisation
{
    public string Id { get; set; }
    public string Name { get; set; }

    public ICollection<Repository> Repositories { get; }
}