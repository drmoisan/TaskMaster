using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Builds immutable folder tree snapshots from primitive hierarchy reader output.
    /// </summary>
    public sealed class FolderTreeSnapshotBuilder
    {
        private readonly IOutlookFolderHierarchyReader _reader;
        private readonly IDeadlineClock _deadlineClock;
        private readonly IDispatcherYield _dispatcherYield;

        public FolderTreeSnapshotBuilder(IOutlookFolderHierarchyReader reader)
            : this(reader, null, null) { }

        public FolderTreeSnapshotBuilder(
            IOutlookFolderHierarchyReader reader,
            IDeadlineClock deadlineClock,
            IDispatcherYield dispatcherYield
        )
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _deadlineClock = deadlineClock;
            _dispatcherYield = dispatcherYield;
        }

        public async Task<FolderTreeSnapshot> BuildSnapshotAsync(
            FolderTreeRequest request,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nodes = await _reader
                .ReadFoldersAsync(request, _deadlineClock, _dispatcherYield, cancellationToken)
                .ConfigureAwait(false);
            var lookup = nodes.ToDictionary(node => node.Key);
            var roots = nodes
                .Where(node => node.ParentKey == null)
                .Select(node => node.Key)
                .ToArray();
            var ordered = new List<FolderTreeSnapshotNode>();
            var stack = new Stack<FolderTreeNodeKey>(roots.Reverse());

            while (stack.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await YieldIfNeededAsync(cancellationToken).ConfigureAwait(false);
                var key = stack.Pop();
                if (!lookup.TryGetValue(key, out var node))
                {
                    continue;
                }

                ordered.Add(node);
                foreach (var childKey in node.ChildKeys.Reverse())
                {
                    stack.Push(childKey);
                }
            }

            return new FolderTreeSnapshot(roots, ordered, request);
        }

        private async Task YieldIfNeededAsync(CancellationToken cancellationToken)
        {
            if (_deadlineClock == null || _dispatcherYield == null || !_deadlineClock.ShouldYield())
            {
                return;
            }

            await _dispatcherYield.YieldAsync(cancellationToken).ConfigureAwait(false);
            _deadlineClock.Reset();
        }
    }
}
