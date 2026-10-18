#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>One known folder occurrence and its lazily loaded child state.</summary>
    internal sealed class BreadcrumbDescendantNode
    {
        internal BreadcrumbDescendantNode(FolderBreadcrumbSegment segment)
        {
            Identity = Guid.NewGuid().ToString("N");
            Segment = segment;
        }

        internal string Identity { get; }
        internal FolderBreadcrumbSegment Segment { get; }
        internal bool Expanded { get; set; }
        internal bool Loaded { get; private set; }
        internal int RequestGeneration { get; set; }
        internal IReadOnlyList<BreadcrumbDescendantNode> Children { get; private set; } =
            Array.Empty<BreadcrumbDescendantNode>();

        internal void Attach(IReadOnlyList<FolderBreadcrumbSegment> segments)
        {
            Loaded = true;
            var remaining = Children.ToList();
            Children = segments
                .Select(segment =>
                {
                    var child = remaining.FirstOrDefault(node =>
                        node.Segment.Key.Equals(segment.Key)
                    );
                    if (child == null)
                    {
                        return new BreadcrumbDescendantNode(segment);
                    }
                    remaining.Remove(child);
                    return child;
                })
                .ToArray();
        }

        internal BreadcrumbDescendantNode? Find(string identity)
        {
            if (Identity == identity)
            {
                return this;
            }
            foreach (var child in Children)
            {
                var found = child.Find(identity);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        internal void Collapse()
        {
            Expanded = false;
            RequestGeneration++;
            foreach (var child in Children)
            {
                child.Collapse();
            }
        }
    }
}
