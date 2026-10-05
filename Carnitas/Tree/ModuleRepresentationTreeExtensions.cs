using Sarsoo.Terraform.JsonOutput.Plan;
using Sarsoo.Terraform.JsonOutput.Value;

namespace Carnitas.Tree;

/// <summary>
/// Builds a <see cref="TreeNode{T}"/> forest from a Terraform plan.
/// </summary>
public static class ModuleRepresentationTreeExtensions
{
    /// <summary>
    /// Projects the plan's resource changes into a forest nested by module path. Root-module
    /// resources become top-level leaves; shared module chains are merged into a single branch.
    /// </summary>
    public static IReadOnlyList<TreeNode<ModuleRepresentation>> ToResourceTree(this ModuleRepresentation plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return TreeBuilder.BuildForest(plan.ToResourceTreeInner());
    }
    
    private static IEnumerable<TreeEntry<ModuleRepresentation>> ToResourceTreeInner(this ModuleRepresentation plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var changes = plan.ChildModules;
        
        if (changes is not null)
        {
            foreach (var change in changes)
            {
                var path = new List<string>(TerraformAddress.ParseModulePath(change.Address))
                {
                    // TerraformAddress.GetRelativeAddress(change.Address)
                    change.Address
                };

                yield return new TreeEntry<ModuleRepresentation>(path, change);

                foreach (var child in change.ChildModules ??  Enumerable.Empty<ModuleRepresentation>())
                {
                    foreach (var resource in child.ToResourceTreeInner())
                    {
                        yield return resource;
                    }
                }
            }
        }
    }
}
