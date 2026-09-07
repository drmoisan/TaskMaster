#nullable enable
using System;
using System.Collections.Concurrent;
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

        // Two DISTINCT per-instance structures (#799 D6), not one. The reported set only ever gains
        // entries, which is what makes the AC7 diagnostic once per label per session rather than
        // once per render. The absent set also LOSES entries, because a label that becomes
        // resolvable after a snapshot refresh must stop being suppressed. ConcurrentDictionary
        // rather than HashSet because ResolveLeafKeyAsync awaits AcquireSnapshotAsync and its
        // continuations are not guaranteed to resume on one thread; per-instance rather than static
        // because a static set is process-wide mutable state shared across viewers and across test
        // methods in a single assembly.
        private readonly ConcurrentDictionary<string, byte> _reportedLabels = new(
            StringComparer.OrdinalIgnoreCase
        );

        private readonly ConcurrentDictionary<string, byte> _absentLabels = new(
            StringComparer.OrdinalIgnoreCase
        );

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
            var mapped = MapNodes(chain);

            // The trim runs AFTER the snapshot walk and BEFORE the caller sees the chain, so row
            // order, banner placement and the trash pseudo-row are all untouched (#799 AC1, AC2).
            string? archiveRoot = TryReadArchiveRoot();
            if (string.IsNullOrWhiteSpace(archiveRoot))
            {
                return mapped;
            }

            if (ArchiveChainProjection.TryTrimBelowArchiveRoot(mapped, archiveRoot, out var trimmed))
            {
                return trimmed;
            }

            // AC2: a chain that never reaches the archive root is a diagnosable condition. Returning
            // an empty list routes the Efc surface into the empty-chain single-segment fallback and
            // the QuickFiler surface into its existing scored fallback.
            EmitError(
                $"Resolved ancestor chain does not pass through the configured archive root '{archiveRoot}'; falling back to single-segment rendering."
            );
            return Array.Empty<FolderBreadcrumbSegment>();
        }

        /// <summary>
        /// Reads the configured archive root through the injected accessor, treating a null
        /// accessor and any exception from it alike as "no trim configured". The accessor is lazy
        /// and its faults are swallowed here because the underlying archive-root property throws
        /// when the root is unresolvable and two of the three construction sites are outside any
        /// try block, so a propagating read would create a new throw site at those call sites.
        /// </summary>
        private string? TryReadArchiveRoot()
        {
            var accessor = ArchiveRootAccessor;
            if (accessor is null)
            {
                return null;
            }

            try
            {
                return accessor();
            }
            catch (Exception exception)
            {
                logger.Debug(
                    "The archive-root accessor threw; leaving the ancestor chain untrimmed.",
                    exception
                );
                return null;
            }
        }

        /// <summary>Emits one diagnostic through log4net and through the injected test sink.</summary>
        private void EmitError(string message)
        {
            logger.Error(message);
            ErrorSink?.Invoke(message);
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
                // The exact-path route returns before the suffix pass is ever reached, so the AC7
                // absence signal has to be cleared here as well as on the suffix success route.
                _absentLabels.TryRemove(folderPath, out _);
                return match.Key;
            }

            var resolved = ResolveByUniqueSuffix(snapshot, folderPath);
            if (resolved != null)
            {
                _absentLabels.TryRemove(folderPath, out _);
            }

            return resolved;
        }

        /// <inheritdoc />
        public bool IsAbsentLabel(string folderPath) =>
            !string.IsNullOrWhiteSpace(folderPath) && _absentLabels.ContainsKey(folderPath);

        /// <summary>
        /// Second resolution pass for a relative stem such as <c>Projects\Alpha</c>, which the
        /// QuickFiler surface presents in place of a store-qualified path. Accepts a node whose
        /// full path ends with a directory separator followed by the requested path, and only when
        /// exactly one node qualifies: uniqueness is the safety property that prevents filing into
        /// a same-named folder under a different root. Zero or multiple candidates return null, so
        /// the caller keeps today's single-segment fallback rendering.
        /// <para>
        /// An instance member rather than a static one because the AC7 log gate and the absence
        /// classification are both per-provider-instance state (#799 D6).
        /// </para>
        /// </summary>
        private FolderTreeNodeKey? ResolveByUniqueSuffix(
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

            if (candidates.Length == 0)
            {
                // AC7, restricted by decision D-B to the ZERO-candidate cause: the label is absent
                // from the snapshot. Ambiguity is not absence — the folder does exist — so the
                // multiple-candidate cause deliberately leaves this signal untouched.
                _absentLabels[folderPath] = 0;
            }

            // TryAdd is the AC7 log gate: a label already reported by this provider instance emits
            // nothing further, so the diagnostic is once per label per session rather than once per
            // render. The two causes stay distinguishable in the message text.
            if (_reportedLabels.TryAdd(folderPath, 0))
            {
                EmitError(
                    candidates.Length == 0
                        ? $"No snapshot node path ends with '{suffix}'; leaving '{folderPath}' unresolved."
                        : $"Multiple snapshot node paths end with '{suffix}'; leaving '{folderPath}' unresolved."
                );
            }

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
