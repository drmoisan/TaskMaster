namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Resolves live Outlook folder handles at consumption boundaries only.
    /// Unit tests must mock this adapter and must not create a live Outlook application.
    /// </summary>
    public interface IFolderHandleResolver
    {
        object Resolve(FolderTreeSnapshotNode node);

        bool TryResolve(FolderTreeSnapshotNode node, out object folder);
    }
}
