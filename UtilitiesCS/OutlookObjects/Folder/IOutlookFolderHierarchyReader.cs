using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Reads primitive folder metadata behind the live Outlook COM boundary.
    /// Unit tests must provide fake hierarchy readers rather than live Outlook objects.
    /// </summary>
    public interface IOutlookFolderHierarchyReader
    {
        Task<IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(
            FolderTreeRequest request,
            IDeadlineClock deadlineClock,
            IDispatcherYield dispatcherYield,
            CancellationToken cancellationToken
        );
    }
}
