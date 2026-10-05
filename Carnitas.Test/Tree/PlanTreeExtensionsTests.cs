using Carnitas.Tree;
using Sarsoo.Terraform.JsonOutput.Plan;

namespace Carnitas.Test;

public class PlanTreeExtensionsTests
{
    [Fact]
    public void ToResourceTree_nests_resources_by_module_path()
    {
        var root = Change("aws_instance.root", null);
        var child = Change("module.foo.aws_instance.child", "module.foo");
        var nested = Change("module.foo.module.bar.aws_s3_bucket.data", "module.foo.module.bar");

        var plan = new PlanRepresentation
        {
            ResourceChanges = [root, child, nested]
        };

        var forest = plan.ToResourceChangeTree();

        Assert.Equal(2, forest.Count);
        Assert.Same(root, forest[0].Value);
        Assert.Equal("aws_instance.root", forest[0].Label);

        var foo = forest[1];
        Assert.Equal("module.foo", foo.Label);
        Assert.Null(foo.Value);
        Assert.Equal(2, foo.Children.Count);
        Assert.Same(child, foo.Children[0].Value);
        Assert.Equal("aws_instance.child", foo.Children[0].Label);

        var bar = foo.Children[1];
        Assert.Equal("module.bar", bar.Label);
        Assert.Null(bar.Value);
        var leaf = Assert.Single(bar.Children);
        Assert.Same(nested, leaf.Value);
        Assert.Equal("aws_s3_bucket.data", leaf.Label);
    }

    [Fact]
    public void ToResourceTree_merges_modules_shared_by_multiple_resources()
    {
        var plan = new PlanRepresentation
        {
            ResourceChanges =
            [
                Change("module.foo.aws_instance.a", "module.foo"),
                Change("module.foo.aws_instance.b", "module.foo")
            ]
        };

        var forest = plan.ToResourceChangeTree();

        var foo = Assert.Single(forest);
        Assert.Equal("module.foo", foo.Label);
        Assert.Equal(2, foo.Children.Count);
        Assert.Equal(["aws_instance.a", "aws_instance.b"], foo.Children.Select(c => c.Label));
    }

    [Fact]
    public void ToResourceTree_handles_null_resource_changes()
    {
        var plan = new PlanRepresentation { ResourceChanges = null! };

        Assert.Empty(plan.ToResourceChangeTree());
    }

    private static ResourceChange Change(string address, string? moduleAddress) => new()
    {
        Address = address,
        ModuleAddress = moduleAddress
    };
}
