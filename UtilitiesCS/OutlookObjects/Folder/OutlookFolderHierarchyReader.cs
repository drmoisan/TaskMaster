#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Store;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Reads primitive folder metadata from included Outlook stores.
    /// </summary>
    public sealed class OutlookFolderHierarchyReader : IOutlookFolderHierarchyReader
    {
        private readonly Func<IEnumerable<IOutlookStoreAdapter>> _storeProvider;
        private readonly StoresWrapper _storesWrapper;

        [ExcludeFromCodeCoverage]
        public OutlookFolderHierarchyReader(
            Outlook.NameSpace namespaceMapi,
            StoresWrapper storesWrapper
        )
            : this(
                () =>
                    namespaceMapi
                        .Stores.Cast<Outlook.Store>()
                        .Select(store => (IOutlookStoreAdapter)new OutlookStoreAdapter(store)),
                storesWrapper
            ) { }

        internal OutlookFolderHierarchyReader(
            Func<IEnumerable<IOutlookStoreAdapter>> storeProvider,
            StoresWrapper storesWrapper
        )
        {
            _storeProvider =
                storeProvider ?? throw new ArgumentNullException(nameof(storeProvider));
            _storesWrapper =
                storesWrapper ?? throw new ArgumentNullException(nameof(storesWrapper));
        }

        [ExcludeFromCodeCoverage]
        public async Task<IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(
            FolderTreeRequest? request,
            IDeadlineClock? deadlineClock,
            IDispatcherYield? dispatcherYield,
            CancellationToken cancellationToken
        )
        {
            var records = await ReadRecordsAsync(
                request,
                deadlineClock,
                dispatcherYield,
                cancellationToken
            );
            return records.Select(record => ToNode(record, records)).ToArray();
        }

        [ExcludeFromCodeCoverage]
        public IReadOnlyList<OutlookFolderHierarchyRecord> ReadRecords(
            FolderTreeRequest? request,
            CancellationToken cancellationToken
        )
        {
            return ReadRecordsAsync(request, null, null, cancellationToken)
                .GetAwaiter()
                .GetResult();
        }

        [ExcludeFromCodeCoverage]
        public async Task<IReadOnlyList<OutlookFolderHierarchyRecord>> ReadRecordsAsync(
            FolderTreeRequest? request,
            IDeadlineClock? deadlineClock,
            IDispatcherYield? dispatcherYield,
            CancellationToken cancellationToken
        )
        {
            var records = new List<OutlookFolderHierarchyRecord>();
            foreach (var store in _storeProvider())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await YieldIfNeededAsync(deadlineClock, dispatcherYield, cancellationToken);
                if (!store.ShouldInclude(_storesWrapper))
                {
                    continue;
                }

                var storeId = store.StoreId;
                if (request != null && !request.IncludesStore(storeId))
                {
                    continue;
                }

                var root = store.GetRootFolder();
                if (root == null)
                {
                    continue;
                }

                await ReadStoreAsync(
                    root,
                    storeId,
                    records,
                    deadlineClock,
                    dispatcherYield,
                    cancellationToken
                );
            }

            return records;
        }

        [ExcludeFromCodeCoverage]
        private static async Task ReadStoreAsync(
            IOutlookFolderAdapter root,
            string storeId,
            ICollection<OutlookFolderHierarchyRecord> records,
            IDeadlineClock? deadlineClock,
            IDispatcherYield? dispatcherYield,
            CancellationToken cancellationToken
        )
        {
            var stack = new Stack<Tuple<IOutlookFolderAdapter, string, string>>();
            stack.Push(Tuple.Create(root, string.Empty, root.FolderPath));

            while (stack.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await YieldIfNeededAsync(deadlineClock, dispatcherYield, cancellationToken);
                var current = stack.Pop();
                var folder = current.Item1;
                var parentEntryId = current.Item2;
                var rootPath = current.Item3;
                records.Add(
                    new OutlookFolderHierarchyRecord(
                        storeId,
                        folder.EntryID,
                        parentEntryId,
                        folder.Name,
                        folder.FolderPath,
                        GetRelativePath(rootPath, folder)
                    )
                );

                var children = folder.Children;
                if (children == null || children.Count == 0)
                {
                    continue;
                }

                foreach (var child in children.Reverse())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    stack.Push(Tuple.Create(child, folder.EntryID, rootPath));
                }
            }
        }

        private static async Task YieldIfNeededAsync(
            IDeadlineClock? deadlineClock,
            IDispatcherYield? dispatcherYield,
            CancellationToken cancellationToken
        )
        {
            if (deadlineClock == null || dispatcherYield == null || !deadlineClock.ShouldYield())
            {
                return;
            }

            await dispatcherYield.YieldAsync(cancellationToken);
            deadlineClock.Reset();
        }

        [ExcludeFromCodeCoverage]
        private static FolderTreeSnapshotNode ToNode(
            OutlookFolderHierarchyRecord record,
            IEnumerable<OutlookFolderHierarchyRecord> records
        )
        {
            var childKeys = records
                .Where(item =>
                    item.StoreId == record.StoreId && item.ParentEntryId == record.EntryId
                )
                .Select(item => item.Key)
                .ToArray();
            var parent = records.FirstOrDefault(item =>
                item.StoreId == record.StoreId && item.EntryId == record.ParentEntryId
            );
            return new FolderTreeSnapshotNode(
                record.Key,
                record.DisplayName,
                record.StoreId,
                record.EntryId,
                parent?.Key,
                record.FolderPath,
                record.RelativePath,
                childKeys,
                false,
                string.Empty
            );
        }

        private static string GetRelativePath(string rootPath, IOutlookFolderAdapter folder)
        {
            return string.Equals(rootPath, folder.FolderPath, StringComparison.OrdinalIgnoreCase)
                ? folder.Name
                : folder.FolderPath.Replace(rootPath + "\\", string.Empty);
        }

        internal interface IOutlookStoreAdapter
        {
            string StoreId { get; }
            bool ShouldInclude(StoresWrapper storesWrapper);
            IOutlookFolderAdapter? GetRootFolder();
        }

        internal interface IOutlookFolderAdapter
        {
            string EntryID { get; }
            string Name { get; }
            string FolderPath { get; }
            IReadOnlyList<IOutlookFolderAdapter> Children { get; }
        }

        [ExcludeFromCodeCoverage]
        private sealed class OutlookStoreAdapter : IOutlookStoreAdapter
        {
            private readonly Outlook.Store _store;

            public OutlookStoreAdapter(Outlook.Store store)
            {
                _store = store ?? throw new ArgumentNullException(nameof(store));
            }

            public string StoreId => _store.StoreID;

            public bool ShouldInclude(StoresWrapper storesWrapper)
            {
                return storesWrapper.ShouldIncludeStore(_store);
            }

            public IOutlookFolderAdapter? GetRootFolder()
            {
                return _store.GetRootFolder() is Outlook.MAPIFolder folder
                    ? new OutlookFolderAdapter(folder)
                    : null;
            }
        }

        [ExcludeFromCodeCoverage]
        private sealed class OutlookFolderAdapter : IOutlookFolderAdapter
        {
            private readonly Outlook.MAPIFolder _folder;

            public OutlookFolderAdapter(Outlook.MAPIFolder folder)
            {
                _folder = folder ?? throw new ArgumentNullException(nameof(folder));
            }

            public string EntryID => _folder.EntryID;
            public string Name => _folder.Name;
            public string FolderPath => _folder.FolderPath;

            public IReadOnlyList<IOutlookFolderAdapter> Children =>
                _folder
                    .Folders.Cast<Outlook.MAPIFolder>()
                    .Select(folder => (IOutlookFolderAdapter)new OutlookFolderAdapter(folder))
                    .ToArray();
        }
    }
}
