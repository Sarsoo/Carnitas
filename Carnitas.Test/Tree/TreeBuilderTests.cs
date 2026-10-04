using Carnitas.Tree;

namespace Carnitas.Test;

public class TreeBuilderTests
{
    [Fact]
    public void Builds_nested_forest_and_merges_shared_prefixes()
    {
        var forest = TreeBuilder.BuildForest(new[]
        {
            new TreeEntry<string>(["a", "b"], "one"),
            new TreeEntry<string>(["a", "c"], "two"),
            new TreeEntry<string>(["d"], "three")
        });

        Assert.Equal(2, forest.Count);
        Assert.Equal("a", forest[0].Label);
        Assert.Equal("d", forest[1].Label);

        Assert.Equal(2, forest[0].Children.Count);
        Assert.Equal("b", forest[0].Children[0].Label);
        Assert.Equal("one", forest[0].Children[0].Value);
        Assert.Equal("c", forest[0].Children[1].Label);
        Assert.Equal("two", forest[0].Children[1].Value);

        Assert.Equal("three", forest[1].Value);
    }

    [Fact]
    public void Preserves_first_seen_sibling_order()
    {
        var forest = TreeBuilder.BuildForest(new[]
        {
            new TreeEntry<string>(["z", "1"], "a"),
            new TreeEntry<string>(["y", "1"], "b"),
            new TreeEntry<string>(["z", "2"], "c")
        });

        Assert.Equal(["z", "y"], forest.Select(n => n.Label));
        Assert.Equal(["1", "2"], forest[0].Children.Select(n => n.Label));
    }

    [Fact]
    public void Intermediate_nodes_carry_default_value()
    {
        var forest = TreeBuilder.BuildForest(new[]
        {
            new TreeEntry<string>(["a", "b"], "leaf")
        });

        Assert.Null(forest[0].Value);
        Assert.Equal("leaf", forest[0].Children[0].Value);
        Assert.True(forest[0].HasChildren);
        Assert.False(forest[0].Children[0].HasChildren);
    }

    [Fact]
    public void Overwrites_value_on_duplicate_leaf_path()
    {
        var forest = TreeBuilder.BuildForest(new[]
        {
            new TreeEntry<string>(["a"], "first"),
            new TreeEntry<string>(["a"], "second")
        });

        var node = Assert.Single(forest);
        Assert.Equal("second", node.Value);
    }

    [Fact]
    public void Throws_on_empty_path()
    {
        Assert.Throws<ArgumentException>(() =>
            TreeBuilder.BuildForest(new[] { new TreeEntry<string>([], "value") }));
    }

    [Fact]
    public void Throws_on_empty_segment()
    {
        Assert.Throws<ArgumentException>(() =>
            TreeBuilder.BuildForest(new[] { new TreeEntry<string>(["a", ""], "value") }));
    }

    [Fact]
    public void SelfAndDescendants_returns_depth_first_pre_order()
    {
        var root = new TreeNode<string>("root");
        var child = root.AddChild(new TreeNode<string>("child"));
        child.AddChild(new TreeNode<string>("grandchild"));

        Assert.Equal(["root", "child", "grandchild"], nodeLabels(root.SelfAndDescendants()));
    }

    [Fact]
    public void SelfAndDescendants_handles_deep_chain()
    {
        var root = new TreeNode<string>("level-0");
        var current = root;
        for (var i = 1; i < 60; i++)
        {
            current = current.AddChild(new TreeNode<string>($"level-{i}"));
        }

        Assert.Equal(60, root.SelfAndDescendants().Count());
        Assert.Equal("level-59", root.SelfAndDescendants().Last().Label);
    }

    private static IEnumerable<string> nodeLabels(IEnumerable<TreeNode<string>> nodes) =>
        nodes.Select(n => n.Label);
}
