#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace Carnitas.Model.Governance.Policy;

public class PrincipalResourceFunction
{
    public string PrincipalId { get; set; }
    public PrincipalType PrincipalType { get; set; }

    public string ResourceId { get; set; }
    public ResourceType ResourceType { get; set; }

    public Function Function { get; set; }
    public string FunctionName { get; set; }
}