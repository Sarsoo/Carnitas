using Carnitas.Model.Governance;
using Carnitas.Model.Source.SourceControl.GitHub;

namespace Carnitas.Model.Source.SourceControl;

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

    public ICollection<Module> Modules { get; }
    public ICollection<Checkout> Checkouts { get; }
    
    public ICollection<Operations.InitRun> InitRuns { get; }
    public ICollection<Operations.PlanRun> PlanRuns { get; }
    public ICollection<Operations.ApplyRun> ApplyRuns { get; }
}