using System.Globalization;
using System.Text;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Markdown;

public sealed class UnicodeTreeRenderer
{
    public string Render(IEnumerable<TreeSnapshotNode> roots, bool showCounts = true, int maximumDepth = 20)
    {
        StringBuilder builder = new();
        TreeSnapshotNode[] items = roots.OrderBy(node => node.Name, StringComparer.Ordinal).ToArray();
        for (int index = 0; index < items.Length; index++)
        {
            if (index > 0)
            {
                builder.AppendLine();
            }
            RenderNode(builder, items[index], string.Empty, true, 1, showCounts, maximumDepth, true);
        }
        return builder.ToString().TrimEnd();
    }

    private static void RenderNode(StringBuilder builder, TreeSnapshotNode node, string prefix, bool last, int depth, bool counts, int maximumDepth, bool root)
    {
        if (!root)
        {
            builder.Append(prefix).Append(last ? "└─ " : "├─ ");
        }
        builder.Append(node.Name);
        if (counts)
        {
            builder.Append("  (").Append(node.Count.ToString("N0", CultureInfo.InvariantCulture)).Append(')');
        }
        builder.AppendLine();
        if (depth >= maximumDepth && !node.Children.IsEmpty)
        {
            builder.Append(prefix).Append(root ? string.Empty : last ? "   " : "│  ").Append("└─ …").AppendLine();
            return;
        }
        string childPrefix = prefix + (root ? string.Empty : last ? "   " : "│  ");
        for (int index = 0; index < node.Children.Length; index++)
        {
            RenderNode(builder, node.Children[index], childPrefix, index == node.Children.Length - 1, depth + 1, counts, maximumDepth, false);
        }
    }
}
