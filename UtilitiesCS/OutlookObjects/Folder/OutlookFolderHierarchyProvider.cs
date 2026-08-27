#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Host-neutral facade over <see cref="IOutlookFolderTreeService"/> that projects the cached
    /// <see cref="FolderTreeSnapshot"/> into breadcrumb segments. Adds no COM code and is not
    /// coverage-exempt; the live Outlook query stays isolated behind the injected service interface.
    /// </summary>
    public sealed class OutlookFolderHierarchyProvider : IFolderHierarchyProvider
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private readonly IOutlookFolderTreeService _treeService;

        /// <summary>
        /// Creates a provider over the supplied folder-tree service.
        /// </summary>
        /// <param name="treeService">The cached snapshot service. Required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="treeService"/> is null.</exception>
        public OutlookFolderHierarchyProvider(IOutlookFolderTreeService treeService)
        {
            _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(
            FolderTreeNodeKey leafKey,
            CancellationToken cancellationToken
        )
        {
            var snapshot = await AcquireSnapshotAsync(cancellationToken).ConfigureAwait(false);
            var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, leafKey);
            return MapNodes(chain);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(
            FolderTreeNodeKey segmentKey,
            CancellationToken cancellationToken
        )
        {
            var snapshot = await AcquireSnapshotAsync(cancellationToken).ConfigureAwait(false);
            var children = snapshot.GetChildren(segmentKey);
            return MapNodes(children);
        }

        /// <inheritdoc />
        public async Task<FolderTreeNodeKey?> ResolveLeafKeyAsync(
            string folderPath,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return null;
            }

            var snapshot = await AcquireSnapshotAsync(cancellationToken).ConfigureAwait(false);

            // First-match on duplicate paths is the documented behavior; real Outlook full paths embed
            // the store name and are unique in practice.
            var match = snapshot.NodesByKey.Values.FirstOrDefault(node =>
                string.Equals(node.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase)
            );

            if (match != null)
            {
                return match.Key;
            }

            return ResolveByUniqueSuffix(snapshot, folderPath);
        }

        /// <summary>
        /// Second resolution pass for a relative stem such as <c>Projects\Alpha</c>, which the
        /// QuickFiler surface presents in place of a store-qualified path. Accepts a node whose
        /// full path ends with a directory separator followed by the requested path, and only when
        /// exactly one node qualifies: uniqueness is the safety property that prevents filing into
        /// a same-named folder under a different root. Zero or multiple candidates return null, so
        /// the caller keeps today's single-segment fallback rendering.
        /// </summary>
        private static FolderTreeNodeKey? ResolveByUniqueSuffix(
            FolderTreeSnapshot snapshot,
            string folderPath
        )
        {
            string suffix = "\\" + folderPath;
            var candidates = snapshot
                .NodesByKey.Values.Where(node =>
                    node.FolderPath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                )
                .Take(2)
                .ToArray();

            if (candidates.Length == 1)
            {
                return candidates[0].Key;
            }

            logger.Error(
                candidates.Length == 0
                    ? $"No snapshot node path ends with '{suffix}'; leaving '{folderPath}' unresolved."
                    : $"Multiple snapshot node paths end with '{suffix}'; leaving '{folderPath}' unresolved."
            );
            return null;
        }

        private Task<FolderTreeSnapshot> AcquireSnapshotAsync(CancellationToken cancellationToken)
        {
            return _treeService.GetSnapshotAsync(
                FolderTreeRequest.AllStores(allowStaleSnapshot: true),
                cancellationToken
            );
        }

        private static IReadOnlyList<FolderBreadcrumbSegment> MapNodes(
            IReadOnlyList<FolderTreeSnapshotNode> nodes
        )
        {
            return nodes.Select(MapNode).ToArray();
        }

        private static FolderBreadcrumbSegment MapNode(FolderTreeSnapshotNode node)
        {
            return new FolderBreadcrumbSegment(
                node.Key,
                node.DisplayName,
                node.FolderPath,
                node.ChildKeys.Count > 0
            );
        }
    }
}
