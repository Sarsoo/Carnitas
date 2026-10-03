#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Model.Governance;

public class Organisation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public ICollection<Repository> Repositories { get; }
}