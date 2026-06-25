namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Identifies why a folder tree snapshot build or refresh was requested.
    /// </summary>
    public enum FolderTreeRefreshReason
    {
        InitialLoad,
        ManualRefresh,
        FolderAdded,
        FolderRemoved,
        FolderChanged,
        StoreAdded,
        StoreRemoved,
        Disposal,
    }
}
