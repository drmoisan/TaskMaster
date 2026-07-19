#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Resolves live Outlook folder handles from snapshot metadata at consumption boundaries.
    /// </summary>
    public sealed class OutlookFolderHandleResolver : IFolderHandleResolver
    {
        private readonly IFolderLookup _folderLookup;

        [ExcludeFromCodeCoverage]
        public OutlookFolderHandleResolver(Outlook.NameSpace namespaceMapi)
            : this(new OutlookFolderLookup(namespaceMapi)) { }

        [ExcludeFromCodeCoverage]
        internal OutlookFolderHandleResolver(IFolderLookup folderLookup)
        {
            _folderLookup = folderLookup ?? throw new ArgumentNullException(nameof(folderLookup));
        }

        [ExcludeFromCodeCoverage]
        public object Resolve(FolderTreeSnapshotNode node)
        {
            if (!TryResolve(node, out var folder))
            {
                throw new InvalidOperationException(
                    "The Outlook folder handle could not be resolved."
                );
            }

            return folder!;
        }

        [ExcludeFromCodeCoverage]
        public bool TryResolve(FolderTreeSnapshotNode? node, out object? folder)
        {
            if (node == null)
            {
                folder = null;
                return false;
            }

            folder = _folderLookup.GetFolderFromId(node.EntryId, node.StoreId);
            return folder != null;
        }

        internal interface IFolderLookup
        {
            object GetFolderFromId(string entryId, string storeId);
        }

        [ExcludeFromCodeCoverage]
        private sealed class OutlookFolderLookup : IFolderLookup
        {
            private readonly Outlook.NameSpace _namespaceMapi;

            public OutlookFolderLookup(Outlook.NameSpace namespaceMapi)
            {
                _namespaceMapi =
                    namespaceMapi ?? throw new ArgumentNullException(nameof(namespaceMapi));
            }

            public object GetFolderFromId(string entryId, string storeId)
            {
                return _namespaceMapi.GetFolderFromID(entryId, storeId);
            }
        }
    }
}
