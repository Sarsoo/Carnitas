
using Carnitas.Model.Governance;
using Carnitas.Model.Source.GitHub;

namespace Carnitas.Model.Source;

public class Repository
{
    public string Id { get; set; }
    public string Name { get; set; }

    public RepositoryType Type { get; set; }

    public string? GitHubAppId { get; set; }
    public GitHubApp? GitHubApp { get; set; }

    public string? RepositoryUrl { get; set; }
    public string GitUrl { get; set; }

    public string OrganisationId { get; set; }
    public Organisation Organisation { get; set; }

    public ICollection<RootModule> RootModules { get; }
}