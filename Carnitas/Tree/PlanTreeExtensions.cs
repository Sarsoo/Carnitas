using Sarsoo.Terraform.JsonOutput.Plan;

namespace Carnitas.Tree;

/// <summary>
/// Builds a <see cref="TreeNode{T}"/> forest from a Terraform plan.
/// </summary>
public static class PlanTreeExtensions
{
    /// <summary>
    /// Projects the plan's resource changes into a forest nested by module path. Root-module
    /// resources become top-level leaves; shared module chains are merged into a single branch.
    /// </summary>
    public static IReadOnlyList<TreeNode<ResourceChange>> ToResourceTree(this PlanRepresentation plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var changes = plan.ResourceChanges ?? [];
        var entries = new List<TreeEntry<ResourceChange>>(changes.Count);

        foreach (var change in changes)
        {
            var path = new List<string>(TerraformAddress.ParseModulePath(change.ModuleAddress))
            {
                TerraformAddress.GetRelativeAddress(change)
            };

            entries.Add(new TreeEntry<ResourceChange>(path, change));
        }

        return TreeBuilder.BuildForest(entries);
    }
}
