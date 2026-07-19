#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// The epic's single shared upstream contract for live folder-hierarchy queries. Consumed across
    /// module boundaries by the EfcViewer (9102) and QuickFiler (9103) breadcrumb features.
    /// </summary>
    /// <remarks>
    /// Host-neutral: the only dependency is <see cref="IOutlookFolderTreeService"/> (an interface, not
    /// COM), so the contract is unit-testable without a live Outlook process. All members are
    /// <see cref="Task"/>-returning only because snapshot acquisition is async; the ancestor walk and
    /// children projection themselves are synchronous and independently unit-tested via
    /// <see cref="FolderTreeSnapshotQueries.GetAncestorChain"/>.
    /// </remarks>
    public interface IFolderHierarchyProvider
    {
        /// <summary>
        /// Returns the ordered root-to-leaf ancestor chain for the selected leaf folder.
        /// </summary>
        /// <param name="leafKey">Identity of the selected leaf folder node.</param>
        /// <param name="cancellationToken">Token observed while acquiring the snapshot.</param>
        /// <returns>
        /// Segments ordered root-first / leaf-last, with the last segment equal to the requested leaf.
        /// An empty list (never null) when <paramref name="leafKey"/> is null or absent from the
        /// current snapshot.
        /// </returns>
        Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(
            FolderTreeNodeKey leafKey,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Returns the real immediate subfolders of a given segment, sourced from the live cached
        /// snapshot rather than from suggestion rows.
        /// </summary>
        /// <param name="segmentKey">Identity of the segment whose children are requested.</param>
        /// <param name="cancellationToken">Token observed while acquiring the snapshot.</param>
        /// <returns>
        /// The immediate child segments, or an empty list (never null) when the segment has no children
        /// or <paramref name="segmentKey"/> is unknown.
        /// </returns>
        Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(
            FolderTreeNodeKey segmentKey,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Resolves a UI-selected folder path to a stable node key against the current snapshot.
        /// </summary>
        /// <param name="folderPath">Full folder path selected in the host.</param>
        /// <param name="cancellationToken">Token observed while acquiring the snapshot.</param>
        /// <returns>
        /// The matching <see cref="FolderTreeNodeKey"/>, or <c>null</c> when no matching node exists.
        /// Identity is by key, so duplicate segment names at different depths are distinguished.
        /// </returns>
        Task<FolderTreeNodeKey?> ResolveLeafKeyAsync(
            string folderPath,
            CancellationToken cancellationToken
        );
    }
}
