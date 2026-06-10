namespace Carnitas.Model.Governance.Policy;

public class Function
{
    public string Name { get; set; }

    public ICollection<PrincipalResourceFunction> PrincipalResourceFunctions { get; set; }

    public static readonly string[] StaticData = [
        "sadsad"
    ];
}