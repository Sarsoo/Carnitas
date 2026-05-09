namespace Carnitas.Model.Governance.Policy;

public class PrincipalResourceFunction
{
    public string PrincipalId { get; set; }
    public PrincipalType PrincipalType { get; set; }

    public string ResourceId { get; set; }
    public ResourceType ResourceType { get; set; }

    public Function Function { get; set; }
}