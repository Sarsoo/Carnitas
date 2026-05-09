
using Carnitas.Model.Source;

namespace Carnitas.Model.Governance;

public class Repository
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string OrganisationId { get; set; }
    public Organisation Organisation { get; set; }

    public ICollection<RootModule> RootModules { get; }
}