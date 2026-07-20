using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// COM-free (tier 1) tests for <see cref="StoreRehookCoordinator"/> (issue #263). Drives the
    /// internal <see cref="StoreRehookCoordinator.RehookStoreCoreAsync(string)"/> through injected
    /// delegate/interface seams to exercise all five <see cref="StoreRehookOutcome"/> values,
    /// StoreID-keyed idempotency, ordering of the four primitives behind the readiness gate, and the
    /// public <see cref="StoreRehookCoordinator.RehookAsync(StoreIdentity)"/> adapter. No live
    /// Outlook, no temporary files, no real timers (the inter-attempt delay is a no-op).
    /// </summary>
    [TestClass]
    public sealed class StoreRehookCoordinatorTests
    {
        private const string Identity = "Mailbox - Test";
        private const string StoreId = "store-1";

        private sealed class Harness
        {
            public Mock<IOutlookReadinessGate> Gate { get; } =
                new Mock<IOutlookReadinessGate>(MockBehavior.Strict);
            public Mock<IOutlookFolderNotificationSink> Sink { get; } =
                new Mock<IOutlookFolderNotificationSink>(MockBehavior.Loose);
            public Mock<IOutlookFolderTreeService> TreeService { get; } =
                new Mock<IOutlookFolderTreeService>(MockBehavior.Loose);
            public List<string> Calls { get; } = new List<string>();

            // This file has no project-level <Nullable> and no whole-file #nullable pragma; the
            // pre-existing `?` annotations on these two properties need an explicit annotations
            // context to avoid CS8632. Scoping narrowly to annotations-only avoids introducing
            // new CS86xx diagnostics elsewhere in this file (no behavior change per AC7).
#nullable enable annotations
            public Outlook.Store? ResolvedStore { get; set; }
            public bool AlreadyHooked { get; set; }
            public Exception? AddOrRestoreThrows { get; set; }

#nullable restore annotations

            public StoreRehookCoordinator Build()
            {
                Sink.Setup(s => s.AddStore(It.IsAny<Outlook.Store>()))
                    .Callback(() => Calls.Add("addStore"));
                TreeService
                    .Setup(t =>
                        t.MarkStale(It.IsAny<string>(), It.IsAny<FolderTreeRefreshReason>())
                    )
                    .Callback(() => Calls.Add("markStale"));

                return new StoreRehookCoordinator(
                    Gate.Object,
                    _ => ResolvedStore,
                    _ => AlreadyHooked,
                    _ =>
                    {
                        Calls.Add("addOrRestore");
                        if (AddOrRestoreThrows != null)
                        {
                            throw AddOrRestoreThrows;
                        }
                    },
                    _ => Calls.Add("subscribeInbox"),
                    () => Sink.Object,
                    () => TreeService.Object,
                    _ => Task.CompletedTask
                );
            }
        }

        private static Outlook.Store CreateStore(string storeId)
        {
            var store = new Mock<Outlook.Store>(MockBehavior.Loose);
            store.SetupGet(x => x.StoreID).Returns(storeId);
            return store.Object;
        }

        [TestMethod]
        public async Task RehookStoreCoreAsync_WhenIdentityDoesNotResolve_ReturnsStoreNotFound()
        {
            var harness = new Harness { ResolvedStore = null };
            var sut = harness.Build();

            var result = await sut.RehookStoreCoreAsync(Identity);

            result.Outcome.Should().Be(StoreRehookOutcome.StoreNotFound);
            harness.Calls.Should().BeEmpty("no store means no COM hookup work");
        }

        [TestMethod]
        public async Task RehookStoreCoreAsync_WhenAlreadyFullyHooked_ReturnsAlreadyHookedWithNoPrimitiveCalls()
        {
            var harness = new Harness
            {
                ResolvedStore = CreateStore(StoreId),
                AlreadyHooked = true,
            };
            var sut = harness.Build();

            var result = await sut.RehookStoreCoreAsync(Identity);

            result.Outcome.Should().Be(StoreRehookOutcome.AlreadyHooked);
            result.StoreId.Should().Be(StoreId);
            harness.Calls.Should().BeEmpty("an already-hooked store performs zero primitive calls");
            harness.Gate.Verify(
                g => g.IsReady(It.IsAny<Outlook.Store>()),
                Times.Never(),
                "the readiness gate is not probed for an already-hooked store"
            );
        }

        [TestMethod]
        public async Task RehookStoreCoreAsync_WhenTransientThenReady_ReturnsSuccessAndDrivesPrimitivesInOrder()
        {
            var harness = new Harness { ResolvedStore = CreateStore(StoreId) };
            harness
                .Gate.SetupSequence(g => g.IsReady(It.IsAny<Outlook.Store>()))
                .Returns(false)
                .Returns(false)
                .Returns(true);
            var sut = harness.Build();

            var result = await sut.RehookStoreCoreAsync(Identity);

            result.Outcome.Should().Be(StoreRehookOutcome.Success);
            result.StoreId.Should().Be(StoreId);
            harness.Calls.Should().Equal("addOrRestore", "subscribeInbox", "addStore", "markStale");
            harness.TreeService.Verify(
                t => t.MarkStale(StoreId, FolderTreeRefreshReason.StoreAdded),
                Times.Once()
            );
        }

        [TestMethod]
        public async Task RehookStoreCoreAsync_WhenGateNeverReady_ReturnsTransientTimeoutWithNoEagerComRead()
        {
            var harness = new Harness { ResolvedStore = CreateStore(StoreId) };
            harness.Gate.Setup(g => g.IsReady(It.IsAny<Outlook.Store>())).Returns(false);
            var sut = harness.Build();

            var result = await sut.RehookStoreCoreAsync(Identity);

            result.Outcome.Should().Be(StoreRehookOutcome.TransientTimeout);
            harness
                .Calls.Should()
                .BeEmpty("no expensive COM read occurs before the gate reports ready");
            harness.Gate.Verify(
                g => g.IsReady(It.IsAny<Outlook.Store>()),
                Times.Exactly(StoreRehookCoordinator.MaxReadinessAttempts)
            );
        }

        [TestMethod]
        public async Task RehookStoreCoreAsync_WhenPrimitiveThrowsNonTransient_ReturnsPermanentErrorWithoutThrowing()
        {
            var boom = new InvalidOperationException("hookup failed");
            var harness = new Harness
            {
                ResolvedStore = CreateStore(StoreId),
                AddOrRestoreThrows = boom,
            };
            harness.Gate.Setup(g => g.IsReady(It.IsAny<Outlook.Store>())).Returns(true);
            harness.Gate.Setup(g => g.IsTransientError(It.IsAny<COMException>())).Returns(false);
            var sut = harness.Build();

            var result = await sut.RehookStoreCoreAsync(Identity);

            result.Outcome.Should().Be(StoreRehookOutcome.PermanentError);
            result.Error.Should().BeSameAs(boom, "the causing exception is captured, not thrown");
        }

        [TestMethod]
        public async Task RehookAsync_StoreNotFound_LogsAndReturnsWithoutThrowing()
        {
            var harness = new Harness { ResolvedStore = null };
            var sut = harness.Build();

            Func<Task> act = () => sut.RehookAsync(StoreIdentity.Resolve(Identity));

            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task RehookAsync_TransientTimeout_LogsAndReturnsWithoutThrowing()
        {
            var harness = new Harness { ResolvedStore = CreateStore(StoreId) };
            harness.Gate.Setup(g => g.IsReady(It.IsAny<Outlook.Store>())).Returns(false);
            var sut = harness.Build();

            Func<Task> act = () => sut.RehookAsync(StoreIdentity.Resolve(Identity));

            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task RehookAsync_PermanentErrorFromComException_LogsHResultAndReturnsWithoutThrowing()
        {
            var com = new COMException("boom", unchecked((int)0x8004010F));
            var harness = new Harness
            {
                ResolvedStore = CreateStore(StoreId),
                AddOrRestoreThrows = com,
            };
            harness.Gate.Setup(g => g.IsReady(It.IsAny<Outlook.Store>())).Returns(true);
            harness.Gate.Setup(g => g.IsTransientError(It.IsAny<COMException>())).Returns(false);
            var sut = harness.Build();

            Func<Task> act = () => sut.RehookAsync(StoreIdentity.Resolve(Identity));

            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task RehookAsync_PermanentErrorFromNonComException_LogsAndReturnsWithoutThrowing()
        {
            var harness = new Harness
            {
                ResolvedStore = CreateStore(StoreId),
                AddOrRestoreThrows = new InvalidOperationException("boom"),
            };
            harness.Gate.Setup(g => g.IsReady(It.IsAny<Outlook.Store>())).Returns(true);
            var sut = harness.Build();

            Func<Task> act = () => sut.RehookAsync(StoreIdentity.Resolve(Identity));

            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task RehookAsync_Success_LogsAndReturnsWithoutThrowing()
        {
            var harness = new Harness { ResolvedStore = CreateStore(StoreId) };
            harness.Gate.Setup(g => g.IsReady(It.IsAny<Outlook.Store>())).Returns(true);
            var sut = harness.Build();

            Func<Task> act = () => sut.RehookAsync(StoreIdentity.Resolve(Identity));

            await act.Should().NotThrowAsync();
            harness.Calls.Should().Equal("addOrRestore", "subscribeInbox", "addStore", "markStale");
        }

        [TestMethod]
        public async Task RehookAsync_PublicAdapter_ExtractsIdentityValueDelegatesAndReturnsWithoutThrowing()
        {
            // Same CS8632 annotations-context scoping as above.
#nullable enable annotations
            string? resolvedWith = null;
#nullable restore annotations
            var store = CreateStore(StoreId);
            var coordinator = new StoreRehookCoordinator(
                new Mock<IOutlookReadinessGate>(MockBehavior.Loose).Object,
                id =>
                {
                    resolvedWith = id;
                    return store;
                },
                _ => true, // already hooked → fast, deterministic success variant
                _ => { },
                _ => { },
                () => new Mock<IOutlookFolderNotificationSink>(MockBehavior.Loose).Object,
                () => new Mock<IOutlookFolderTreeService>(MockBehavior.Loose).Object,
                _ => Task.CompletedTask
            );

            var identity = StoreIdentity.Resolve(Identity);
            Func<Task> act = () => coordinator.RehookAsync(identity);

            await act.Should().NotThrowAsync();
            resolvedWith.Should().Be(Identity, "the adapter extracts StoreIdentity.Value");
        }
    }
}
