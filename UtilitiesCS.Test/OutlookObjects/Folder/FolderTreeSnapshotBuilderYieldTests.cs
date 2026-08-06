using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeSnapshotBuilderYieldTests
    {
        [TestMethod]
        public async Task BuildSnapshotAsync_WhenClockRequestsYield_DispatcherYieldRunsPerNode()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                "store-a",
                depth: 2
            );
            var dispatcherYield = new FakeDispatcherYield();
            var builder = new FolderTreeSnapshotBuilder(
                reader,
                new AlwaysYieldClock(),
                dispatcherYield
            );

            await builder.BuildSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            dispatcherYield.YieldCount.Should().Be(3);
        }

        [TestMethod]
        public async Task BuildSnapshotAsync_WhenClockDoesNotRequestYield_DispatcherDoesNotRun()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                "store-a",
                depth: 2
            );
            var dispatcherYield = new FakeDispatcherYield();
            var builder = new FolderTreeSnapshotBuilder(
                reader,
                new NeverYieldClock(),
                dispatcherYield
            );

            await builder.BuildSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            dispatcherYield.YieldCount.Should().Be(0);
        }

        [TestMethod]
        public async Task BuildSnapshotAsync_AfterForcedYield_KeepsSubsequentYieldsOnDispatcher()
        {
            using (var dispatcherHost = new StaDispatcherHost())
            {
                var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                    "store-a",
                    depth: 2
                );
                var dispatcherYield = new ThreadRecordingDispatcherYield();
                var builder = new FolderTreeSnapshotBuilder(
                    reader,
                    new AlwaysYieldClock(),
                    dispatcherYield
                );
                var buildTask = dispatcherHost
                    .Dispatcher.InvokeAsync(() =>
                        builder.BuildSnapshotAsync(
                            FolderTreeRequest.AllStores(false),
                            CancellationToken.None
                        )
                    )
                    .Task;

                await await buildTask;

                dispatcherYield.ThreadIds.Should().OnlyContain(id => id == dispatcherHost.ThreadId);
            }
        }

        private sealed class AlwaysYieldClock : IDeadlineClock
        {
            public bool ShouldYield() => true;

            public void Reset() { }
        }

        private sealed class NeverYieldClock : IDeadlineClock
        {
            public bool ShouldYield() => false;

            public void Reset() { }
        }

        private sealed class ThreadRecordingDispatcherYield : IDispatcherYield
        {
            private readonly List<int> _threadIds = new List<int>();

            public IReadOnlyList<int> ThreadIds => _threadIds;

            public async Task YieldAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                await Dispatcher.Yield(DispatcherPriority.Background);
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
