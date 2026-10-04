namespace Carnitas.Tree;

/// <summary>
/// A generic, mutable tree node. Intermediate nodes may carry no payload (<c>default</c>),
/// while leaf nodes typically hold the value the tree was built from.
/// </summary>
/// <typeparam name="T">The type of payload stored on a node.</typeparam>
public sealed class TreeNode<T>
{
    private readonly List<TreeNode<T>> _children = [];

    public TreeNode(string label, T? value = default)
    {
        ArgumentNullException.ThrowIfNull(label);
        Label = label;
        Value = value;
    }

    /// <summary>
    /// The display segment for this node (e.g. <c>module.foo</c> or <c>aws_instance.web</c>).
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// The payload associated with this node, or <c>default</c> for intermediate nodes.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Child nodes in insertion order.
    /// </summary>
    public IReadOnlyList<TreeNode<T>> Children => _children;

    /// <summary>
    /// Whether this node has any children.
    /// </summary>
    public bool HasChildren => _children.Count > 0;

    /// <summary>
    /// Appends a pre-built child node. Duplicate labels are allowed.
    /// </summary>
    public TreeNode<T> AddChild(TreeNode<T> child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
        return child;
    }

    /// <summary>
    /// Returns the existing child with an ordinal-equal label, or appends and returns a new one.
    /// </summary>
    public TreeNode<T> GetOrAddChild(string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        foreach (var child in _children)
        {
            if (string.Equals(child.Label, label, StringComparison.Ordinal))
            {
                return child;
            }
        }

        var created = new TreeNode<T>(label);
        _children.Add(created);
        return created;
    }

    /// <summary>
    /// Enumerates this node followed by all of its descendants in depth-first, pre-order.
    /// </summary>
    public IEnumerable<TreeNode<T>> SelfAndDescendants()
    {
        yield return this;

        foreach (var child in _children)
        {
            foreach (var descendant in child.SelfAndDescendants())
            {
                yield return descendant;
            }
        }
    }
}
