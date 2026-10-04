using Sarsoo.Terraform.JsonOutput.Plan;

namespace Carnitas.Tree;

/// <summary>
/// Helpers for interpreting Terraform resource and module addresses.
/// </summary>
public static class TerraformAddress
{
    private const string ModulePrefix = "module.";

    /// <summary>
    /// Parses a Terraform module address into ordered, display-ready segments, each including the
    /// literal <c>module.</c> prefix. Instance keys are preserved verbatim and may contain dots.
    /// </summary>
    /// <example>
    /// <c>module.foo.module.bar[0].module.baz["k.y"]</c> becomes
    /// <c>["module.foo", "module.bar[0]", "module.baz[\"k.y\"]"]</c>.
    /// </example>
    public static IReadOnlyList<string> ParseModulePath(string? moduleAddress)
    {
        if (string.IsNullOrWhiteSpace(moduleAddress))
        {
            return Array.Empty<string>();
        }

        var segments = new List<string>();
        var span = moduleAddress.AsSpan();
        var position = 0;

        while (position < span.Length)
        {
            while (position < span.Length && span[position] == '.')
            {
                position++;
            }

            if (position >= span.Length || !span[position..].StartsWith(ModulePrefix, StringComparison.Ordinal))
            {
                break;
            }

            var start = position;
            position += ModulePrefix.Length;

            while (position < span.Length && span[position] != '.' && span[position] != '[')
            {
                position++;
            }

            if (position < span.Length && span[position] == '[')
            {
                var inQuotes = false;
                var escaped = false;

                while (position < span.Length)
                {
                    var character = span[position];

                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (character == '\\' && inQuotes)
                    {
                        escaped = true;
                    }
                    else if (character == '"')
                    {
                        inQuotes = !inQuotes;
                    }
                    else if (character == ']' && !inQuotes)
                    {
                        position++;
                        break;
                    }

                    position++;
                }
            }

            segments.Add(span[start..position].ToString());
        }

        return segments;
    }

    /// <summary>
    /// Returns the module-relative address of a resource. For root-module resources this is the
    /// full address; when the address is missing it falls back to <c>Type.Name[index]</c>.
    /// </summary>
    public static string GetRelativeAddress(ResourceChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var address = change.Address;
        if (string.IsNullOrEmpty(address))
        {
            var fallback = string.Join('.', new[] { change.Type, change.Name }.Where(s => !string.IsNullOrEmpty(s)));
            return change.Index is null ? fallback : $"{fallback}[{change.Index}]";
        }

        var moduleAddress = change.ModuleAddress;
        if (!string.IsNullOrWhiteSpace(moduleAddress)
            && address.StartsWith(moduleAddress + ".", StringComparison.Ordinal))
        {
            return address[(moduleAddress.Length + 1)..];
        }

        return address;
    }
}
