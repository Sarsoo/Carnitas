using Carnitas.Tree;
using MudBlazor;
using Sarsoo.Terraform.JsonOutput.Value;

namespace Carnitas.Web.Components.MachineReadableUI;

public record class ModuleTreeContext(ModuleRepresentation? Module, ResourceRepresentation? Resource);

/// <summary>
/// Maps a <see cref="TreeNode{T}"/> resource forest onto MudBlazor tree item data. Kept separate
/// from <c>PlanTree.razor</c> so the presentation logic can be unit tested.
/// </summary>
public static class ModuleRepresentationTreeModel
{
    /// <summary>
    /// Maps the whole forest to MudBlazor items, preserving order.
    /// </summary>
    public static IReadOnlyList<TreeItemData<ModuleTreeContext>> Build(
        IReadOnlyList<TreeNode<ModuleRepresentation>> forest)
    {
        ArgumentNullException.ThrowIfNull(forest);
        return forest.SelectMany(Map).ToList();
    }

    /// <summary>
    /// Maps a single node: module branches get a folder icon and start collapsed; resource leaves
    /// carry the change and its action icon.
    /// </summary>
    public static IEnumerable<TreeItemData<ModuleTreeContext>> Map(TreeNode<ModuleRepresentation> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var children = node.Children.SelectMany(Map).ToList();

        if (node.Value is { } resource)
        {
            yield return new TreeItemData<ModuleTreeContext>
            {
                Text = node.Label,
                Value = new(resource, null),
                // Icon = ReduceAction(resource).Icon,
                Children = children
            };

            foreach (var childResource in resource.Resources ?? Enumerable.Empty<ResourceRepresentation>())
            {
                yield return new TreeItemData<ModuleTreeContext>
                {
                    Text = childResource.Address,
                    Value = new(null, childResource),
                    // Icon = ReduceAction(resource).Icon,
                };
            }
        }
        else
        {
            yield return new TreeItemData<ModuleTreeContext>
            {
                Text = node.Label,
                Icon = Icons.Material.Filled.Folder,
                Expanded = false,
                Children = children
            };
        }
    }

    /// <summary>
    /// Reduces a resource's raw action list to the single action shown in the tree.
    /// A create combined with a delete is a replacement; otherwise the first action is used.
    /// </summary>
    // public static ResourceAction ReduceAction(ModuleRepresentation change)
    // {
    //     ArgumentNullException.ThrowIfNull(change);
    //
    //     var actions = change.Change?.Actions;
    //     if (actions is null || actions.Count == 0)
    //     {
    //         return ResourceAction.NoOp;
    //     }
    //
    //     if (actions.Contains(ResourceAction.Delete) && actions.Contains(ResourceAction.Create))
    //     {
    //         return ResourceAction.Replace;
    //     }
    //
    //     return actions[0];
    // }

    /// <summary>
    /// The colour for a node's icon: module branches use the default colour, resource leaves use
    /// the action colour.
    /// </summary>
    public static Color GetColour(ModuleRepresentation? change) => Color.Default;

    /// <summary>
    /// The trailing text for a node: none for module branches, the action name for resource leaves.
    /// </summary>
    // public static string? GetActionText(ModuleRepresentation? change) =>
    //     change is null ? null : ReduceAction(change).ToString();
}
