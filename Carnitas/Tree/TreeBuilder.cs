namespace Carnitas.Tree;

/// <summary>
/// A single entry used to build a tree: a path of labels plus the payload that belongs on the
/// final (leaf) node.
/// </summary>
/// <typeparam name="T">The type of payload stored on the leaf node.</typeparam>
public readonly record struct TreeEntry<T>(IReadOnlyList<string> Path, T Value);

/// <summary>
/// Builds <see cref="TreeNode{T}"/> forests from flat path/value entries. Shared path prefixes are
/// merged so a single branch can carry many leaves.
/// </summary>
public static class TreeBuilder
{
    /// <summary>
    /// Builds a forest from <paramref name="entries"/>. Every path segment except the last
    /// creates or finds an intermediate node; the last segment's node receives the entry value.
    /// Top-level and sibling order follow first appearance in <paramref name="entries"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A path is empty, or contains a null/empty segment.</exception>
    public static IReadOnlyList<TreeNode<T>> BuildForest<T>(IEnumerable<TreeEntry<T>> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var roots = new List<TreeNode<T>>();

        foreach (var entry in entries)
        {
            var path = entry.Path;
            if (path is null || path.Count == 0)
            {
                throw new ArgumentException("A tree entry must have at least one path segment.", nameof(entries));
            }

            TreeNode<T>? parent = null;
            for (var i = 0; i < path.Count; i++)
            {
                var segment = path[i];
                if (string.IsNullOrEmpty(segment))
                {
                    throw new ArgumentException("Path segments must be non-empty.", nameof(entries));
                }

                var node = parent is null ? GetOrAdd(roots, segment) : parent.GetOrAddChild(segment);

                if (i == path.Count - 1)
                {
                    node.Value = entry.Value;
                }

                parent = node;
            }
        }

        return roots;
    }

    private static TreeNode<T> GetOrAdd<T>(List<TreeNode<T>> nodes, string label)
    {
        foreach (var node in nodes)
        {
            if (string.Equals(node.Label, label, StringComparison.Ordinal))
            {
                return node;
            }
        }

        var created = new TreeNode<T>(label);
        nodes.Add(created);
        return created;
    }
}
