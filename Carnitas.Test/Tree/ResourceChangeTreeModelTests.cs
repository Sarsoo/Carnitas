using Carnitas.Tree;
using Carnitas.Web.Components.MachineReadableUI;
using Carnitas.Web.Components.MachineReadableUI.Plan.ResourceChange;
using Carnitas.Web.Extensions;
using MudBlazor;
using Sarsoo.Terraform.JsonOutput.Change;
using Sarsoo.Terraform.JsonOutput.Plan;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Test;

public class ResourceChangeTreeModelTests
{
    [Fact]
    public void Build_maps_modules_to_collapsed_folder_nodes()
    {
        var plan = new PlanRepresentation
        {
            ResourceChanges =
            [
                Change("module.foo.aws_instance.create", "module.foo", ResourceAction.Create)
            ]
        };

        var items = ResourceChangeTreeModel.Build(plan.ToResourceChangeTree());

        var module = Assert.Single(items);
        Assert.Equal("module.foo", module.Text);
        Assert.Equal(Icons.Material.Filled.Folder, module.Icon);
        Assert.False(module.Expanded);
        Assert.Null(module.Value);
        Assert.Single(module.Children!);
    }

    [Fact]
    public void Build_maps_resource_leaves_to_action_icon_and_text()
    {
        var plan = new PlanRepresentation
        {
            ResourceChanges =
            [
                Change("module.foo.aws_instance.create", "module.foo", ResourceAction.Create),
                Change("module.foo.aws_instance.update", "module.foo", ResourceAction.Update)
            ]
        };

        var module = Assert.Single(ResourceChangeTreeModel.Build(plan.ToResourceChangeTree()));
        var create = module.Children!.First(c => c.Text == "aws_instance.create");
        var update = module.Children!.First(c => c.Text == "aws_instance.update");

        Assert.Equal(ResourceAction.Create.Icon, create.Icon);
        Assert.Equal(Color.Success, ResourceChangeTreeModel.GetColour(create.Value));
        Assert.Equal("Create", ResourceChangeTreeModel.GetActionText(create.Value));

        Assert.Equal(Color.Warning, ResourceChangeTreeModel.GetColour(update.Value));
        Assert.Equal("Update", ResourceChangeTreeModel.GetActionText(update.Value));
    }

    [Fact]
    public void ReduceAction_treats_create_and_delete_as_replace()
    {
        var change = Change("aws_instance.web", null, ResourceAction.Delete, ResourceAction.Create);

        Assert.Equal(ResourceAction.Replace, ResourceChangeTreeModel.ReduceAction(change));
        Assert.Equal(Color.Tertiary, ResourceChangeTreeModel.GetColour(change));
    }

    [Fact]
    public void ReduceAction_defaults_to_no_op_when_change_is_missing()
    {
        var change = new ResourceChange { Address = "aws_instance.web" };

        Assert.Equal(ResourceAction.NoOp, ResourceChangeTreeModel.ReduceAction(change));
        Assert.Equal(Color.Default, ResourceChangeTreeModel.GetColour(change));
    }

    [Fact]
    public void Module_nodes_have_no_action_colour_or_text()
    {
        var plan = new PlanRepresentation
        {
            ResourceChanges = [Change("module.foo.aws_instance.web", "module.foo", ResourceAction.Read)]
        };

        var module = Assert.Single(ResourceChangeTreeModel.Build(plan.ToResourceChangeTree()));

        Assert.Equal(Color.Default, ResourceChangeTreeModel.GetColour(module.Value));
        Assert.Null(ResourceChangeTreeModel.GetActionText(module.Value));
    }

    private static ResourceChange Change(
        string address,
        string? moduleAddress,
        params ResourceAction[] actions) => new()
    {
        Address = address,
        ModuleAddress = moduleAddress,
        Change = actions.Length == 0 ? null : new ChangeRepresentation { Actions = [.. actions] }
    };
}
