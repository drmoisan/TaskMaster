using System.Collections.Generic;
using System.Threading;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Reads primitive folder metadata behind the live Outlook COM boundary.
    /// Unit tests must provide fake hierarchy readers rather than live Outlook objects.
    /// </summary>
    public interface IOutlookFolderHierarchyReader
    {
        IReadOnlyList<FolderTreeSnapshotNode> ReadFolders(
            FolderTreeRequest request,
            CancellationToken cancellationToken
        );
    }
}
