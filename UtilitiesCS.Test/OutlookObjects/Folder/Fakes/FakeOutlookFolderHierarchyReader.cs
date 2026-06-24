using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder.Fakes
{
    public sealed class FakeOutlookFolderHierarchyReader : IOutlookFolderHierarchyReader
    {
        private readonly List<FakeFolderHierarchyRecord> _records =
            new List<FakeFolderHierarchyRecord>();

        public int EnumerationCount { get; private set; }

        public IReadOnlyList<FakeFolderHierarchyRecord> Records => _records;

        public FakeOutlookFolderHierarchyReader AddRecord(FakeFolderHierarchyRecord record)
        {
            _records.Add(record ?? throw new ArgumentNullException(nameof(record)));
            return this;
        }

        public FakeOutlookFolderHierarchyReader AddDuplicatePathStores(string folderPath)
        {
            AddRecord(
                new FakeFolderHierarchyRecord(
                    "store-a",
                    "entry-a",
                    string.Empty,
                    "Inbox",
                    folderPath,
                    "Inbox"
                )
            );
            AddRecord(
                new FakeFolderHierarchyRecord(
                    "store-b",
                    "entry-b",
                    string.Empty,
                    "Inbox",
                    folderPath,
                    "Inbox"
                )
            );
            return this;
        }

        public FakeOutlookFolderHierarchyReader AddDeepHierarchy(string storeId, int depth)
        {
            var parentEntryId = string.Empty;
            var path = "\\Root";
            AddRecord(
                new FakeFolderHierarchyRecord(
                    storeId,
                    "entry-0",
                    parentEntryId,
                    "Root",
                    path,
                    "Root"
                )
            );
            parentEntryId = "entry-0";

            for (var index = 1; index <= depth; index++)
            {
                var entryId = "entry-" + index;
                var name = "Child" + index;
                path += "\\" + name;
                AddRecord(
                    new FakeFolderHierarchyRecord(
                        storeId,
                        entryId,
                        parentEntryId,
                        name,
                        path,
                        path.Trim('\\')
                    )
                );
                parentEntryId = entryId;
            }

            return this;
        }

        public IReadOnlyList<FolderTreeSnapshotNode> ReadFolders(
            FolderTreeRequest request,
            CancellationToken cancellationToken
        )
        {
            EnumerationCount++;
            var records = _records
                .Where(record => request == null || request.IncludesStore(record.StoreId))
                .ToArray();

            return records
                .Select(record =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var childKeys = records
                        .Where(child =>
                            string.Equals(
                                child.StoreId,
                                record.StoreId,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && string.Equals(
                                child.ParentEntryId,
                                record.EntryId,
                                StringComparison.Ordinal
                            )
                        )
                        .Select(child => child.Key)
                        .ToArray();
                    var parent = records.FirstOrDefault(candidate =>
                        string.Equals(
                            candidate.StoreId,
                            record.StoreId,
                            StringComparison.OrdinalIgnoreCase
                        )
                        && string.Equals(
                            candidate.EntryId,
                            record.ParentEntryId,
                            StringComparison.Ordinal
                        )
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
                })
                .ToArray();
        }
    }
}
