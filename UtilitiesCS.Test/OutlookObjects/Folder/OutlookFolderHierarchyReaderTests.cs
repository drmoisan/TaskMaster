using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class OutlookFolderHierarchyReaderTests
    {
        [TestMethod]
        public void ReadRecords_IncludedStore_ReadsPrimitiveRootMetadata()
        {
            var folder = CreateFolder("entry-a", "Inbox", "\\Inbox");
            var store = CreateStore("store-a", include: true, folder.Object);
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new StoresWrapper { ExcludedStoreNameContains = new List<string>() }
            );

            var records = reader.ReadRecords(FolderTreeRequest.AllStores(false), default);

            records.Should().ContainSingle();
            records[0].StoreId.Should().Be("store-a");
            records[0].EntryId.Should().Be("entry-a");
            records[0].DisplayName.Should().Be("Inbox");
            records[0].FolderPath.Should().Be("\\Inbox");
        }

        [TestMethod]
        public void ReadRecords_ExcludedStore_DoesNotReadRootFolder()
        {
            var folder = CreateFolder("entry-a", "Inbox", "\\Inbox");
            var store = CreateStore("store-a", include: false, folder.Object);
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new StoresWrapper { ExcludedStoreNameContains = new List<string> { "Archive" } }
            );

            var records = reader.ReadRecords(FolderTreeRequest.AllStores(false), default);

            records.Should().BeEmpty();
            store.Verify(item => item.GetRootFolder(), Times.Never);
        }

        [TestMethod]
        public async Task ReadRecordsAsync_WhenClockRequestsYield_YieldsBeforeDeepHierarchyIsFullyMaterialized()
        {
            var root = RecordingFolder.CreateDeepHierarchy("root", depth: 5);
            var store = CreateStore("store-a", include: true, root);
            var dispatcherYield = new MaterializationObservingDispatcherYield(() =>
                root.MaterializedNodeCount
            );
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new StoresWrapper { ExcludedStoreNameContains = new List<string>() }
            );

            var records = await reader.ReadRecordsAsync(
                FolderTreeRequest.AllStores(false),
                new AlwaysYieldClock(),
                dispatcherYield,
                CancellationToken.None
            );

            records.Should().HaveCount(6);
            dispatcherYield
                .MaterializedCountsAtYield.Should()
                .Contain(
                    count => count < records.Count,
                    "live traversal must yield before all hierarchy records are materialized"
                );
        }

        [TestMethod]
        public async Task ReadRecordsAsync_CanceledAtTraversalYield_ThrowsBeforeFullMaterialization()
        {
            var root = RecordingFolder.CreateDeepHierarchy("root", depth: 5);
            var store = CreateStore("store-a", include: true, root);
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new StoresWrapper { ExcludedStoreNameContains = new List<string>() }
            );

            Func<Task> act = () =>
                reader.ReadRecordsAsync(
                    FolderTreeRequest.AllStores(false),
                    new AlwaysYieldClock(),
                    new CancelingDispatcherYield(),
                    CancellationToken.None
                );

            await act.Should().ThrowAsync<OperationCanceledException>();
            root.MaterializedNodeCount.Should().BeLessThan(6);
        }

        [TestMethod]
        public async Task ReadRecordsAsync_WhenClockRequestsYield_ResetsClockAfterDispatcherYield()
        {
            var root = RecordingFolder.CreateDeepHierarchy("root", depth: 2);
            var store = CreateStore("store-a", include: true, root);
            var clock = new ResetCountingClock();
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new StoresWrapper { ExcludedStoreNameContains = new List<string>() }
            );

            await reader.ReadRecordsAsync(
                FolderTreeRequest.AllStores(false),
                clock,
                new MaterializationObservingDispatcherYield(() => root.MaterializedNodeCount),
                CancellationToken.None
            );

            clock.ResetCount.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public async Task ReadRecordsAsync_AfterForcedYield_KeepsFolderAccessOnDispatcher()
        {
            using (var dispatcherHost = new StaDispatcherHost())
            {
                var folder = new ThreadRecordingFolder();
                var store = CreateStore("store-a", include: true, folder);
                var reader = new OutlookFolderHierarchyReader(
                    () => new[] { store.Object },
                    new StoresWrapper { ExcludedStoreNameContains = new List<string>() }
                );
                var dispatcherYield = new WorkerCompletingDispatcherYield();

                var readTask = dispatcherHost
                    .Dispatcher.InvokeAsync(() =>
                        reader.ReadRecordsAsync(
                            FolderTreeRequest.AllStores(false),
                            new AlwaysYieldClock(),
                            dispatcherYield,
                            CancellationToken.None
                        )
                    )
                    .Task;

                await dispatcherYield.Entered;
                await Task.Run(dispatcherYield.Release);
                await (await readTask);

                folder.AccessThreadIds.Should().OnlyContain(id => id == dispatcherHost.ThreadId);
            }
        }

        [TestMethod]
        public void HierarchyRecord_TrimsRequiredValuesAndCreatesKey()
        {
            var record = new OutlookFolderHierarchyRecord(
                " store-a ",
                " entry-a ",
                null,
                " Inbox ",
                " \\Inbox ",
                null
            );

            record.StoreId.Should().Be("store-a");
            record.EntryId.Should().Be("entry-a");
            record.ParentEntryId.Should().BeEmpty();
            record.DisplayName.Should().Be("Inbox");
            record.FolderPath.Should().Be("\\Inbox");
            record.RelativePath.Should().BeEmpty();
            record.Key.FolderPath.Should().Be("\\Inbox");
        }

        [TestMethod]
        public void HierarchyRecord_BlankRequiredValues_Throw()
        {
            Action blankStore = () =>
                new OutlookFolderHierarchyRecord(" ", "entry-a", "", "Inbox", "\\Inbox", "Inbox");
            Action blankEntry = () =>
                new OutlookFolderHierarchyRecord("store-a", " ", "", "Inbox", "\\Inbox", "Inbox");
            Action blankFolderPath = () =>
                new OutlookFolderHierarchyRecord("store-a", "entry-a", "", "Inbox", " ", "Inbox");

            blankStore.Should().Throw<ArgumentException>().WithParameterName("storeId");
            blankEntry.Should().Throw<ArgumentException>().WithParameterName("entryId");
            blankFolderPath.Should().Throw<ArgumentException>().WithParameterName("folderPath");
        }

        private static Mock<OutlookFolderHierarchyReader.IOutlookStoreAdapter> CreateStore(
            string storeId,
            bool include,
            OutlookFolderHierarchyReader.IOutlookFolderAdapter root
        )
        {
            var store = new Mock<OutlookFolderHierarchyReader.IOutlookStoreAdapter>();
            store.SetupGet(item => item.StoreId).Returns(storeId);
            store.Setup(item => item.ShouldInclude(It.IsAny<StoresWrapper>())).Returns(include);
            store.Setup(item => item.GetRootFolder()).Returns(root);
            return store;
        }

        private static Mock<OutlookFolderHierarchyReader.IOutlookFolderAdapter> CreateFolder(
            string entryId,
            string name,
            string path
        )
        {
            var folder = new Mock<OutlookFolderHierarchyReader.IOutlookFolderAdapter>();
            folder.SetupGet(item => item.EntryID).Returns(entryId);
            folder.SetupGet(item => item.Name).Returns(name);
            folder.SetupGet(item => item.FolderPath).Returns(path);
            folder
                .SetupGet(item => item.Children)
                .Returns(new OutlookFolderHierarchyReader.IOutlookFolderAdapter[0]);
            return folder;
        }

        private sealed class AlwaysYieldClock : IDeadlineClock
        {
            public bool ShouldYield() => true;

            public void Reset() { }
        }

        private sealed class ResetCountingClock : IDeadlineClock
        {
            public int ResetCount { get; private set; }

            public bool ShouldYield() => true;

            public void Reset()
            {
                ResetCount++;
            }
        }

        private sealed class CancelingDispatcherYield : IDispatcherYield
        {
            public Task YieldAsync(CancellationToken cancellationToken)
            {
                throw new OperationCanceledException();
            }
        }

        private sealed class MaterializationObservingDispatcherYield : IDispatcherYield
        {
            private readonly Func<int> _materializedNodeCount;

            public MaterializationObservingDispatcherYield(Func<int> materializedNodeCount)
            {
                _materializedNodeCount =
                    materializedNodeCount
                    ?? throw new ArgumentNullException(nameof(materializedNodeCount));
            }

            public IReadOnlyList<int> MaterializedCountsAtYield => _materializedCountsAtYield;

            private readonly List<int> _materializedCountsAtYield = new List<int>();

            public Task YieldAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _materializedCountsAtYield.Add(_materializedNodeCount());
                return Task.CompletedTask;
            }
        }

        private sealed class RecordingFolder : OutlookFolderHierarchyReader.IOutlookFolderAdapter
        {
            private readonly List<RecordingFolder> _children = new List<RecordingFolder>();
            private readonly RecordingFolder _root;
            private bool _materialized;

            private RecordingFolder(
                string entryId,
                string name,
                string folderPath,
                RecordingFolder root
            )
            {
                EntryIdValue = entryId;
                Name = name;
                FolderPath = folderPath;
                _root = root ?? this;
            }

            public string EntryIdValue { get; }

            public string EntryID
            {
                get
                {
                    if (!_materialized)
                    {
                        _materialized = true;
                        _root.MaterializedNodeCount++;
                    }

                    return EntryIdValue;
                }
            }

            public string Name { get; }

            public string FolderPath { get; }

            public int MaterializedNodeCount { get; private set; }

            public IReadOnlyList<OutlookFolderHierarchyReader.IOutlookFolderAdapter> Children =>
                _children;

            public static RecordingFolder CreateDeepHierarchy(string rootName, int depth)
            {
                var root = new RecordingFolder(rootName, rootName, "\\" + rootName, null);
                var current = root;
                foreach (var index in Enumerable.Range(1, depth))
                {
                    var child = new RecordingFolder(
                        "node-" + index,
                        "Node " + index,
                        current.FolderPath + "\\Node " + index,
                        root
                    );
                    current._children.Add(child);
                    current = child;
                }

                return root;
            }
        }

        private sealed class ThreadRecordingFolder
            : OutlookFolderHierarchyReader.IOutlookFolderAdapter
        {
            private readonly List<int> _accessThreadIds = new List<int>();

            public IReadOnlyList<int> AccessThreadIds => _accessThreadIds;

            public string EntryID
            {
                get
                {
                    _accessThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                    return "entry-a";
                }
            }

            public string Name
            {
                get
                {
                    _accessThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                    return "Inbox";
                }
            }

            public string FolderPath
            {
                get
                {
                    _accessThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                    return "\\Inbox";
                }
            }

            public IReadOnlyList<OutlookFolderHierarchyReader.IOutlookFolderAdapter> Children
            {
                get
                {
                    _accessThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                    return new OutlookFolderHierarchyReader.IOutlookFolderAdapter[0];
                }
            }
        }

        private sealed class WorkerCompletingDispatcherYield : IDispatcherYield
        {
            private readonly TaskCompletionSource<bool> _entered = new TaskCompletionSource<bool>();
            private readonly TaskCompletionSource<bool> _released = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            public Task Entered => _entered.Task;

            public Task YieldAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _entered.TrySetResult(true);
                return _released.Task;
            }

            public void Release()
            {
                _released.TrySetResult(true);
            }
        }

        private sealed class StaDispatcherHost : IDisposable
        {
            private readonly AutoResetEvent _ready = new AutoResetEvent(false);
            private readonly Thread _thread;

            public StaDispatcherHost()
            {
                _thread = new Thread(() =>
                {
                    Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                    ThreadId = Thread.CurrentThread.ManagedThreadId;
                    _ready.Set();
                    System.Windows.Threading.Dispatcher.Run();
                });
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
                _ready.WaitOne();
            }

            public Dispatcher Dispatcher { get; private set; }

            public int ThreadId { get; private set; }

            public void Dispose()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                _thread.Join();
                _ready.Dispose();
            }
        }
    }
}
