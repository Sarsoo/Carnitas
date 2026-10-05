using Carnitas.Tree;
using Carnitas.Web.Extensions;
using MudBlazor;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Web.Components.MachineReadableUI.Plan.ResourceChange;

/// <summary>
/// Maps a <see cref="TreeNode{T}"/> resource forest onto MudBlazor tree item data. Kept separate
/// from <c>PlanTree.razor</c> so the presentation logic can be unit tested.
/// </summary>
public static class ResourceChangeTreeModel
{
    /// <summary>
    /// Maps the whole forest to MudBlazor items, preserving order.
    /// </summary>
    public static IReadOnlyList<TreeItemData<Sarsoo.Terraform.JsonOutput.Plan.ResourceChange>> Build(
        IReadOnlyList<TreeNode<Sarsoo.Terraform.JsonOutput.Plan.ResourceChange>> forest)
    {
        ArgumentNullException.ThrowIfNull(forest);
        return forest.Select(Map).ToList();
    }

    /// <summary>
    /// Maps a single node: module branches get a folder icon and start collapsed; resource leaves
    /// carry the change and its action icon.
    /// </summary>
    public static TreeItemData<Sarsoo.Terraform.JsonOutput.Plan.ResourceChange> Map(TreeNode<Sarsoo.Terraform.JsonOutput.Plan.ResourceChange> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var children = node.Children.Select(Map).ToList();

        if (node.Value is { } resource)
        {
            return new TreeItemData<Sarsoo.Terraform.JsonOutput.Plan.ResourceChange>
            {
                Text = node.Label,
                Value = resource,
                Icon = ReduceAction(resource).Icon,
                Children = children
            };
        }

        return new TreeItemData<Sarsoo.Terraform.JsonOutput.Plan.ResourceChange>
        {
            Text = node.Label,
            Icon = Icons.Material.Filled.Folder,
            Expanded = false,
            Children = children
        };
    }

    /// <summary>
    /// Reduces a resource's raw action list to the single action shown in the tree.
    /// A create combined with a delete is a replacement; otherwise the first action is used.
    /// </summary>
    public static ResourceAction ReduceAction(Sarsoo.Terraform.JsonOutput.Plan.ResourceChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var actions = change.Change?.Actions;
        if (actions is null || actions.Count == 0)
        {
            return ResourceAction.NoOp;
        }

        if (actions.Contains(ResourceAction.Delete) && actions.Contains(ResourceAction.Create))
        {
            return ResourceAction.Replace;
        }

        return actions[0];
    }

    /// <summary>
    /// The colour for a node's icon: module branches use the default colour, resource leaves use
    /// the action colour.
    /// </summary>
    public static Color GetColour(Sarsoo.Terraform.JsonOutput.Plan.ResourceChange? change) =>
        change is null ? Color.Default : ReduceAction(change).Colour;

    /// <summary>
    /// The trailing text for a node: none for module branches, the action name for resource leaves.
    /// </summary>
    public static string? GetActionText(Sarsoo.Terraform.JsonOutput.Plan.ResourceChange? change) =>
        change is null ? null : ReduceAction(change).ToString();
}
