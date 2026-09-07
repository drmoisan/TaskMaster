#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Reports whether a folder label was classified as ABSENT from the folder snapshot, meaning
    /// resolution found ZERO candidate nodes for it (#799 AC7). An ambiguous label, for which
    /// resolution found more than one candidate, is deliberately NOT absent.
    /// <para>
    /// This is a separate, small interface rather than a fourth member on
    /// <see cref="IFolderHierarchyProvider"/> for two reasons. net48 has no default interface
    /// members, so a fourth member would break every implementer. Decisively, every breadcrumb
    /// router test constructs a strict <c>Mock&lt;IFolderHierarchyProvider&gt;</c>, which would
    /// throw the first time production called an un-set-up new member; because a strict mock is
    /// simply not an <see cref="IFolderLabelAbsenceReport"/>, the consuming cast yields null and
    /// the AC7 suppression stays inert in every existing router test.
    /// </para>
    /// </summary>
    public interface IFolderLabelAbsenceReport
    {
        /// <summary>
        /// Reports whether <paramref name="folderPath"/> was classified as absent from the
        /// snapshot by the most recent resolution attempt against this instance.
        /// </summary>
        /// <param name="folderPath">The presented folder path or label.</param>
        /// <returns>True when the label resolved to zero candidates; otherwise false.</returns>
        bool IsAbsentLabel(string folderPath);
    }

    /// <summary>
    /// Host-neutral facade over <see cref="IOutlookFolderTreeService"/> that projects the cached
    /// <see cref="FolderTreeSnapshot"/> into breadcrumb segments. Adds no COM code and is not
    /// coverage-exempt; the live Outlook query stays isolated behind the injected service interface.
    /// </summary>
    public sealed class OutlookFolderHierarchyProvider
        : IFolderHierarchyProvider,
            IFolderLabelAbsenceReport
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private readonly IOutlookFolderTreeService _treeService;

        /// <summary>
        /// Creates a provider over the supplied folder-tree service.
        /// </summary>
        /// <param name="treeService">The cached snapshot service. Required.</param>
        /// <param name="archiveRootAccessor">
        /// Optional lazy accessor for the configured Outlook archive root, used to trim the
        /// ancestor chain to the lineage below that root (#799 AC1, AC2). It is a delegate rather
        /// than a value because the underlying archive-root property throws when the root is
        /// unresolvable, and reading it eagerly at construction would create a new throw site at
        /// every construction site. A null accessor is the effective off switch and leaves the
        /// chain untrimmed.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="treeService"/> is null.</exception>
        public OutlookFolderHierarchyProvider(
            IOutlookFolderTreeService treeService,
            System.Func<string>? archiveRootAccessor = null
        )
        {
            _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
            ArchiveRootAccessor = archiveRootAccessor;
        }

        /// <summary>
        /// The lazy archive-root accessor supplied at construction, or null when no trim is
        /// configured. Stored as a get-only auto-property rather than a private readonly field so
        /// that the seam-only intermediate state raises no CS0414 assigned-but-never-read warning.
        /// </summary>
        internal System.Func<string>? ArchiveRootAccessor { get; }

        /// <summary>
        /// Injected diagnostic sink that tests observe INSTEAD of attaching a log4net appender, so
        /// no test mutates the process-global logger repository. Production leaves it null and the
        /// provider logs through its own <c>log4net</c> logger only.
        /// </summary>
        internal System.Action<string>? ErrorSink { get; set; }

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

        /// <inheritdoc />
        public bool IsAbsentLabel(string folderPath) =>
            throw new NotImplementedException(
                "Issue #799: the absence report body is supplied by [P2-T6]."
            );

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
