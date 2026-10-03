#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace Carnitas.Model.Governance.Policy;

public class Function
{
    public string Name { get; set; }

    public ICollection<PrincipalResourceFunction> PrincipalResourceFunctions { get; set; }

    public static readonly string[] StaticData = [
        "sadsad"
    ];
}