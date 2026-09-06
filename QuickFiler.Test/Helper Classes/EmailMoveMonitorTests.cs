using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers.Tests;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Helper_Classes.Tests
{
    /// <summary>
    /// Unit tests for <see cref="EmailMoveMonitor"/> hook/unhook bookkeeping. Outlook COM access
    /// is exercised only through an injected synchronous pass-through marshal delegate so the tests
    /// are deterministic and require no live Outlook process. Each test asserts that COM-dependent
    /// work flows through the marshal delegate.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class EmailMoveMonitorTests
    {
        // UiThread.Dispatcher is process-global, set-once static state. These tests never invoke
        // the default (production) marshal delegate, so they do not depend on UiThread being
        // initialized. To guarantee order-independence even if a future change touches the static
        // path, the setup/teardown below snapshots the static dispatcher field through the shared
        // QuickFiler.Test dispatcher fixture and asserts it is unchanged after each test. The
        // class is not parallelized because other QuickFiler tests intentionally replace
        // UiThread.Dispatcher with dedicated WPF dispatchers.
        //
        // The snapshot reads the private _dispatcher backing field rather than the public
        // Dispatcher property (issue #584): the property getter now throws
        // InvalidOperationException when the field is null, and PropertyInfo.GetValue would
        // surface that as a TargetInvocationException from this class's setup and teardown.
        // Reading the field observes the same state without invoking the guard.
        private Dispatcher _capturedDispatcher;

        /// <summary>
        /// Counts how many times the injected marshal delegate was invoked.
        /// </summary>
        private int _marshalInvocationCount;

        [TestInitialize]
        public void Setup()
        {
            // Snapshot the static UiThread.Dispatcher through the fixture accessor so teardown
            // can confirm no test mutated this set-once static state.
            _capturedDispatcher = UiThreadDispatcherFixture.Current;
            _marshalInvocationCount = 0;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Assert the static dispatcher snapshot is unchanged so any accidental static mutation
            // is caught and tests remain order-independent.
            Dispatcher current = UiThreadDispatcherFixture.Current;
            current.Should().BeSameAs(_capturedDispatcher);
        }

        /// <summary>Synchronous pass-through marshal delegate that records invocation count.</summary>
        private Action<System.Action> CountingPassThrough()
        {
            return action =>
            {
                _marshalInvocationCount++;
                action();
            };
        }

        private static Mock<MailItem> CreateMail(string entryId, Folder parent)
        {
            var mail = new Mock<MailItem>(MockBehavior.Loose);
            mail.SetupGet(x => x.EntryID).Returns(entryId);
            mail.SetupGet(x => x.Parent).Returns(parent);
            return mail;
        }

        private static Mock<Folder> CreateFolder(string entryId)
        {
            var folder = new Mock<Folder>(MockBehavior.Loose);
            folder.SetupGet(x => x.EntryID).Returns(entryId);
            return folder;
        }

        [TestMethod]
        public void HookItem_FirstItemOfFolder_SubscribesBeforeItemMoveOnce_AndSharedFolderDoesNotResubscribe()
        {
            // Arrange: two mail items sharing one folder.
            var folder = CreateFolder("folder-1");
            var mail1 = CreateMail("mail-1", folder.Object);
            var mail2 = CreateMail("mail-2", folder.Object);
            var monitor = new EmailMoveMonitor(CountingPassThrough());

            // Act
            monitor.HookItem(mail1.Object, _ => { });
            monitor.HookItem(mail2.Object, _ => { });

            // Assert: BeforeItemMove subscribed exactly once for the shared folder.
            folder.VerifyAdd(
                f => f.BeforeItemMove += It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Once
            );
        }

        [TestMethod]
        public void UnhookItem_RemovesLastItemForFolder_UnsubscribesBeforeItemMoveOnlyOnLastItem()
        {
            // Arrange: two items in the same folder, both hooked.
            var folder = CreateFolder("folder-1");
            var mail1 = CreateMail("mail-1", folder.Object);
            var mail2 = CreateMail("mail-2", folder.Object);
            var monitor = new EmailMoveMonitor(CountingPassThrough());
            monitor.HookItem(mail1.Object, _ => { });
            monitor.HookItem(mail2.Object, _ => { });

            // Act: remove first item (one remains) then the second (last for folder).
            monitor.UnhookItem(mail1.Object);
            folder.VerifyRemove(
                f => f.BeforeItemMove -= It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Never
            );

            monitor.UnhookItem(mail2.Object);

            // Assert: unsubscribe happened exactly once, only when the last item was removed.
            folder.VerifyRemove(
                f => f.BeforeItemMove -= It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Once
            );
        }

        [TestMethod]
        public void UnhookItem_Null_IsNoOp_NoComAccessNoMarshalInvocation()
        {
            // Arrange
            var monitor = new EmailMoveMonitor(CountingPassThrough());

            // Act
            monitor.UnhookItem(null);

            // Assert: the marshal delegate was never invoked for the null call.
            _marshalInvocationCount.Should().Be(0);
        }

        [TestMethod]
        public void UnhookItem_UsesCachedEntryIds_RemovesExactlyTheMatchingEntry()
        {
            // Arrange: two distinct mail items in the same folder.
            var folder = CreateFolder("folder-1");
            var mail1 = CreateMail("mail-1", folder.Object);
            var mail2 = CreateMail("mail-2", folder.Object);
            var monitor = new EmailMoveMonitor(CountingPassThrough());
            monitor.HookItem(mail1.Object, _ => { });
            monitor.HookItem(mail2.Object, _ => { });

            // Act: unhook mail1. mail2 remains, so its folder must NOT be unsubscribed.
            monitor.UnhookItem(mail1.Object);

            // Assert: the matching entry (mail1) is removed; unsubscribe did not fire because mail2
            // still occupies the folder (proves the cached FolderEntryId count path).
            folder.VerifyRemove(
                f => f.BeforeItemMove -= It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Never
            );

            // Removing mail2 (now last) unsubscribes exactly once.
            monitor.UnhookItem(mail2.Object);
            folder.VerifyRemove(
                f => f.BeforeItemMove -= It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Once
            );
        }

        [TestMethod]
        public void AllComAccess_FlowsThroughInjectedMarshalDelegate()
        {
            // Arrange
            var folder = CreateFolder("folder-1");
            var mail = CreateMail("mail-1", folder.Object);
            var monitor = new EmailMoveMonitor(CountingPassThrough());

            // Act: each COM-touching operation must increment the marshal counter.
            monitor.HookItem(mail.Object, _ => { });
            int afterHook = _marshalInvocationCount;

            monitor.UnhookItem(mail.Object);
            int afterUnhook = _marshalInvocationCount;

            monitor.UnhookAll();
            int afterUnhookAll = _marshalInvocationCount;

            // Assert: every operation routed COM access through the delegate (monotonic increase).
            afterHook.Should().Be(1, "HookItem must marshal its COM access exactly once");
            afterUnhook.Should().Be(2, "UnhookItem must marshal its COM access exactly once");
            afterUnhookAll.Should().Be(3, "UnhookAll must marshal its COM access exactly once");
        }

        [TestMethod]
        public void UnhookAll_UnsubscribesEveryFolder_AndClearsState()
        {
            // Arrange: two items in two different folders.
            var folderA = CreateFolder("folder-A");
            var folderB = CreateFolder("folder-B");
            var mailA = CreateMail("mail-A", folderA.Object);
            var mailB = CreateMail("mail-B", folderB.Object);
            var monitor = new EmailMoveMonitor(CountingPassThrough());
            monitor.HookItem(mailA.Object, _ => { });
            monitor.HookItem(mailB.Object, _ => { });

            // Act
            monitor.UnhookAll();

            // Assert: each folder unsubscribed once during UnhookAll.
            folderA.VerifyRemove(
                f => f.BeforeItemMove -= It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Once
            );
            folderB.VerifyRemove(
                f => f.BeforeItemMove -= It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Once
            );

            // State is cleared: a subsequent UnhookItem of a previously hooked item is a no-op
            // (no further unsubscribe occurs because bookkeeping is empty).
            monitor.UnhookItem(mailA.Object);
            folderA.VerifyRemove(
                f => f.BeforeItemMove -= It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Once
            );
        }

        [TestMethod]
        public void DuplicateHookOfSameItem_AndUnhookNeverHookedItem_DoNotThrowOrSpuriouslyUnsubscribe()
        {
            // Arrange
            var folder = CreateFolder("folder-1");
            var mail = CreateMail("mail-1", folder.Object);
            var neverHooked = CreateMail("mail-never", folder.Object);
            var monitor = new EmailMoveMonitor(CountingPassThrough());

            // Act / Assert: duplicate hook of the same item does not throw.
            System.Action duplicateHook = () =>
            {
                monitor.HookItem(mail.Object, _ => { });
                monitor.HookItem(mail.Object, _ => { });
            };
            duplicateHook.Should().NotThrow();

            // Subscribe occurs once for the first item of the folder (duplicate shares the folder).
            folder.VerifyAdd(
                f => f.BeforeItemMove += It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Once
            );

            // Unhooking an item that was never hooked is a no-op: no exception, no unsubscribe.
            System.Action unhookNeverHooked = () => monitor.UnhookItem(neverHooked.Object);
            unhookNeverHooked.Should().NotThrow();
            folder.VerifyRemove(
                f => f.BeforeItemMove -= It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(),
                Times.Never
            );
        }

        [TestMethod]
        public void UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread()
        {
            // Arrange: a marshal delegate that records the thread on which the COM-access body runs,
            // and runs that body on a dedicated marshal-target thread (simulating the STA thread)
            // rather than the invoking ThreadPool thread. This proves the self-marshaling contract
            // that fixes the cross-thread COM defect (AC1).
            int marshalTargetThreadId = Thread.CurrentThread.ManagedThreadId;
            int recordedBodyThreadId = -1;
            Action<System.Action> marshalToTarget = action =>
            {
                // Execute the body via a fresh dedicated thread, capturing the thread it runs on.
                var t = new Thread(() =>
                {
                    recordedBodyThreadId = Thread.CurrentThread.ManagedThreadId;
                    action();
                });
                t.Start();
                t.Join();
            };

            var folder = CreateFolder("folder-1");
            var mail = CreateMail("mail-1", folder.Object);
            var monitor = new EmailMoveMonitor(marshalToTarget);
            monitor.HookItem(mail.Object, _ => { });

            int callingThreadId = -1;

            // Act: invoke UnhookItem from a ThreadPool thread.
            Task.Run(() =>
                {
                    callingThreadId = Thread.CurrentThread.ManagedThreadId;
                    monitor.UnhookItem(mail.Object);
                })
                .GetAwaiter()
                .GetResult();

            // Assert: the COM-access body executed on the marshal-target (dedicated) thread, NOT on
            // the invoking ThreadPool thread.
            recordedBodyThreadId.Should().NotBe(-1, "the marshaled body must have executed");
            recordedBodyThreadId
                .Should()
                .NotBe(
                    callingThreadId,
                    "COM access must not run on the invoking ThreadPool thread"
                );
        }
    }
}
