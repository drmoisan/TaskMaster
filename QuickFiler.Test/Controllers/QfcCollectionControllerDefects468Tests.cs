using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for the issue #468 defect family that need no COM, no live Outlook, no
    /// WinForms control, and no STA apartment. Covers issue #474 defect 1, issue #286, the issue
    /// #471 pure arithmetic, issue #473 defect 1, and issue #474 defect 2.
    /// <para>
    /// A companion file, <c>QfcCollectionController.TestSupport.cs</c>, carries the shared asserting
    /// reflection helpers and the uninitialized-controller builder.
    /// </para>
    /// </summary>
    [TestClass]
    public partial class QfcCollectionControllerDefects468Tests
    {
        /// <summary>
        /// Name of the private static reentrancy counter guarded by issue #286.
        /// </summary>
        private const string ReentrancyCounterField = "removespecificcontrolgroupcounter";

        /// <summary>
        /// Resets the process-wide reentrancy counter before every test in this class.
        /// </summary>
        /// <remarks>
        /// <c>QfcCollectionController.removespecificcontrolgroupcounter</c> is a private
        /// <em>static</em> field, so its value survives between tests and between test classes. The
        /// General Unit Test Policy requires tests to run in any order without affecting each other,
        /// so the counter is reset both before and after every test rather than only once.
        /// </remarks>
        [TestInitialize]
        public void ResetReentrancyCounterBeforeTest()
        {
            FieldInfo counter = typeof(QfcCollectionController).GetField(
                ReentrancyCounterField,
                BindingFlags.NonPublic | BindingFlags.Static
            );
            counter
                .Should()
                .NotBeNull(
                    because: "the private static field '"
                        + ReentrancyCounterField
                        + "' must exist on QfcCollectionController for the issue #286 tests to be "
                        + "able to observe the reentrancy counter"
                );
            counter.SetValue(null, 0);
        }

        /// <summary>
        /// Resets the process-wide reentrancy counter after every test in this class, so a test that
        /// deliberately leaks the counter cannot contaminate any later test.
        /// </summary>
        [TestCleanup]
        public void ResetReentrancyCounterAfterTest()
        {
            FieldInfo counter = typeof(QfcCollectionController).GetField(
                ReentrancyCounterField,
                BindingFlags.NonPublic | BindingFlags.Static
            );
            counter
                .Should()
                .NotBeNull(
                    because: "the private static field '"
                        + ReentrancyCounterField
                        + "' must exist on QfcCollectionController so a leaked counter cannot "
                        + "contaminate a later test"
                );
            counter.SetValue(null, 0);
        }

        /// <summary>
        /// Reads the private static reentrancy counter, asserting first that the field was found.
        /// </summary>
        private static int ReadReentrancyCounter()
        {
            FieldInfo counter = typeof(QfcCollectionController).GetField(
                ReentrancyCounterField,
                BindingFlags.NonPublic | BindingFlags.Static
            );
            counter
                .Should()
                .NotBeNull(
                    because: "the private static field '"
                        + ReentrancyCounterField
                        + "' must exist on QfcCollectionController"
                );
            return (int)counter.GetValue(null);
        }

        /// <summary>
        /// Issue #474 defect 1. Structural test: the private <c>_parent</c> field and the fifth
        /// constructor parameter are both declared
        /// <see cref="QuickFiler.Controllers.IQfcFormController"/>, not
        /// <c>QuickFiler.Interfaces.IFilerFormController</c>. The assertion is structural because
        /// the defect's symptom, an <c>InvalidCastException</c> from the
        /// <c>(QfcFormController)_parent</c> downcast, sits behind <c>UiThread.Init()</c>, which
        /// shows a window the unit-test policy prohibits.
        /// </summary>
        [TestMethod]
        public void ParentFieldAndConstructorParameterAreTypedIQfcFormController()
        {
            // Arrange
            Type expected = typeof(QuickFiler.Controllers.IQfcFormController);
            FieldInfo parentField = QfcCollectionControllerTestSupport.GetFieldInfo("_parent");
            ConstructorInfo[] constructors = typeof(QfcCollectionController).GetConstructors();
            constructors
                .Should()
                .ContainSingle(
                    because: "QfcCollectionController declares exactly one public constructor"
                );
            ParameterInfo[] parameters = constructors[0].GetParameters();
            parameters
                .Length.Should()
                .BeGreaterThanOrEqualTo(
                    5,
                    because: "the parent collaborator is constructor parameter 5"
                );

            // Act
            string fieldTypeName = parentField.FieldType.FullName;
            string parameterTypeName = parameters[4].ParameterType.FullName;

            // Assert
            fieldTypeName
                .Should()
                .Be(
                    expected.FullName,
                    because: "issue #474 defect 1 requires the _parent field to be declared as "
                        + "QuickFiler.Controllers.IQfcFormController so the runtime downcast to the "
                        + "internal concrete QfcFormController is removed"
                );
            parameterTypeName
                .Should()
                .Be(
                    expected.FullName,
                    because: "issue #474 defect 1 requires constructor parameter 5 to be declared as "
                        + "QuickFiler.Controllers.IQfcFormController so the widening is enforced at "
                        + "every construction site"
                );
        }

        /// <summary>
        /// Issue #286. The reentrancy counter must be restored when
        /// <c>RemoveSpecificControlGroupAsync</c> throws early in its body, just after the
        /// <c>Interlocked.Increment</c>. An uninitialized controller leaves <c>_itemGroups</c>
        /// <c>null</c>; since issue #644 replaced the count-bounded unregister loop with a key
        /// ledger, <c>UnregisterNavigation()</c> no longer reads that field and completes, so the
        /// <see cref="NullReferenceException"/> now originates one statement later, at the
        /// <c>_itemGroups[selection - 1]</c> dereference inside
        /// <c>RemoveSpecificControlGroupAsync</c>. Expected outcome is unchanged: the exception
        /// propagates and the counter is back at its pre-call value. Before the fix the decrement
        /// was the method's last statement and unreachable after a throw, leaking the counter.
        /// </summary>
        [TestMethod]
        public async Task RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            int before = ReadReentrancyCounter();

            // Act
            Func<Task> act = () => controller.RemoveSpecificControlGroupAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<NullReferenceException>(
                    because: "the null _itemGroups field is dereferenced at _itemGroups[selection - 1] "
                        + "inside RemoveSpecificControlGroupAsync, so the decrement must run on that path"
                );
            ReadReentrancyCounter()
                .Should()
                .Be(
                    before,
                    because: "issue #286 requires the Interlocked.Decrement to run on the "
                        + "exceptional exit path, so the counter must return to its pre-call value"
                );
        }

        /// <summary>
        /// Issue #286. Companion to
        /// <see cref="RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter"/>:
        /// the restored decrement must cover the whole body, not only the first statement.
        /// <c>UnregisterNavigation()</c> is arranged to succeed and the throw is raised several
        /// statements later by the mocked <c>IsActiveUI</c> getter. A fix guarding only the first
        /// statement would pass the companion test and fail this one.
        /// </summary>
        [TestMethod]
        public async Task RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();

            // A real (empty) KbdActions instance rather than a mock. Since issue #644 replaced the
            // count-bounded loop with a ledger, UnregisterNavigation iterates an empty ledger here
            // and calls Remove zero times; the real instance is retained so the arrangement stays
            // valid, not because UnregisterNavigation still calls Remove on it.
            Mock<IQfcKeyboardHandler> keyboardHandler = new Mock<IQfcKeyboardHandler>(
                MockBehavior.Loose
            );
            keyboardHandler
                .SetupGet(handler => handler.StringActionsAsync)
                .Returns(new KbdActions<string, KaStringAsync, Func<string, Task>>());
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_kbdHandler",
                keyboardHandler.Object
            );

            Mock<IQfcItemController> itemController = new Mock<IQfcItemController>(
                MockBehavior.Loose
            );
            InvalidOperationException expected = new InvalidOperationException(
                "IsActiveUI is deliberately unavailable in this arrangement"
            );
            itemController.SetupGet(item => item.IsActiveUI).Throws(expected);
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_itemGroups",
                new List<QfcItemGroup>
                {
                    new QfcItemGroup { ItemController = itemController.Object },
                }
            );

            int before = ReadReentrancyCounter();

            // Act
            Func<Task> act = () => controller.RemoveSpecificControlGroupAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>(
                    because: "the mocked IsActiveUI getter is reached several statements after the "
                        + "Interlocked.Increment, once UnregisterNavigation has already completed"
                );
            ReadReentrancyCounter()
                .Should()
                .Be(
                    before,
                    because: "issue #286 requires the Interlocked.Decrement to cover the whole body "
                        + "between the increment and the normal exit, not merely the first statement"
                );
        }

        /// <summary>
        /// Issue #471. <c>ShrinkByRows</c> with a positive row count must reduce the height by the
        /// template row height times that count, rounded once, leaving the width untouched.
        /// </summary>
        /// <remarks>
        /// Two cases are asserted. The first is a whole-pixel row height. The second is a
        /// half-pixel row height, which pins the rounding mode: <c>Math.Round</c> defaults to
        /// <c>MidpointRounding.ToEven</c>, so 20.5 rounds to 20, not to 21.
        /// </remarks>
        [TestMethod]
        public void ShrinkByRows_WithPositiveRemovalCount_ReducesHeight()
        {
            // Arrange
            var start = new System.Drawing.Size(300, 200);

            // Act
            System.Drawing.Size wholePixelRows = QfcCollectionController.ShrinkByRows(
                start,
                25f,
                3
            );
            System.Drawing.Size midpointRow = QfcCollectionController.ShrinkByRows(start, 20.5f, 1);

            // Assert
            wholePixelRows
                .Height.Should()
                .Be(125, because: "three 25 px rows removed from 200 px must leave exactly 125 px");
            wholePixelRows
                .Width.Should()
                .Be(300, because: "the row arithmetic must never disturb the width");
            midpointRow
                .Height.Should()
                .Be(
                    180,
                    because: "Math.Round defaults to MidpointRounding.ToEven, so one 20.5 px row "
                        + "rounds to 20 px and 200 px becomes 180 px, not 179 px"
                );
        }

        /// <summary>
        /// Issue #471. <c>ShrinkByRows</c> with a negative row count must <em>increase</em> the
        /// height by the same amount. This sign-agnostic contract is what the insertion path relies
        /// on, and it is also why the helper alone cannot prove the removal call site passes the
        /// right sign — that is covered by the STA test in
        /// <c>QfcCollectionControllerLayout.StaTests.cs</c>.
        /// </summary>
        [TestMethod]
        public void ShrinkByRows_WithNegativeRemovalCount_IncreasesHeight()
        {
            // Arrange
            var start = new System.Drawing.Size(300, 200);

            // Act
            System.Drawing.Size wholePixelRows = QfcCollectionController.ShrinkByRows(
                start,
                25f,
                -3
            );
            System.Drawing.Size midpointRow = QfcCollectionController.ShrinkByRows(
                start,
                20.5f,
                -1
            );

            // Assert
            wholePixelRows
                .Height.Should()
                .Be(
                    275,
                    because: "a row count of -3 at 25 px must grow 200 px to exactly 275 px, the "
                        + "exact mirror of the 125 px produced by a row count of +3"
                );
            wholePixelRows
                .Width.Should()
                .Be(300, because: "the row arithmetic must never disturb the width");
            midpointRow
                .Height.Should()
                .Be(
                    220,
                    because: "MidpointRounding.ToEven is symmetric about zero, so -20.5 rounds to "
                        + "-20 and 200 px becomes 220 px, the exact mirror of the 180 px produced "
                        + "by a row count of +1"
                );
        }

        /// <summary>
        /// Issue #473 defect 1. A task added to <c>BackgroundLoadingTasks</c> while the drain is in
        /// flight must still be awaited. Before the fix the drain snapshotted the bag once and then
        /// replaced the field, so a late arrival was silently discarded and the drain reported
        /// completion while work was still outstanding.
        /// </summary>
        /// <remarks>
        /// The test is fully deterministic and uses no wall-clock wait, no <c>Thread.Sleep</c> and
        /// no <c>Task.Delay</c>. Two <see cref="TaskCompletionSource{TResult}"/> instances stand in
        /// for the two background tasks. The continuation that performs the late add is registered
        /// on the gate <em>before</em> the drain starts and carries
        /// <see cref="TaskContinuationOptions.ExecuteSynchronously"/>, so it runs on the thread
        /// that completes the gate and ahead of the drain's own continuation. An MTA MSTest method
        /// installs no <see cref="System.Threading.SynchronizationContext"/>, so every await in the
        /// chain resumes synchronously on that same thread; by the time
        /// <c>gate.SetResult</c> returns, the drain has either completed or committed to waiting,
        /// and reading <c>IsCompleted</c> is therefore a settled observation rather than a race.
        /// </remarks>
        [TestMethod]
        public async Task DrainBackgroundLoadingTasksAsync_AwaitsATaskAddedDuringTheDrainWindow()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            var gate = new TaskCompletionSource<bool>();
            var lateArrival = new TaskCompletionSource<bool>();
            controller.BackgroundLoadingTasks = new ConcurrentBag<Task>();
            controller.BackgroundLoadingTasks.Add(gate.Task);
            // The continuation itself is never awaited; it exists only to mutate the bag at the
            // exact moment the gate completes. Discarding the returned task documents that.
            _ = gate.Task.ContinueWith(
                completedGate => controller.BackgroundLoadingTasks.Add(lateArrival.Task),
                TaskContinuationOptions.ExecuteSynchronously
            );
            Task drain = controller.DrainBackgroundLoadingTasksAsync();

            // Act
            gate.SetResult(true);

            // Assert
            drain
                .IsCompleted.Should()
                .BeFalse(
                    because: "a task added to BackgroundLoadingTasks during the drain window is "
                        + "still outstanding work, so the drain must not report completion until "
                        + "that task has also finished"
                );

            // Release the outstanding work so the test leaves no pending task behind.
            lateArrival.SetResult(true);
            await drain;
        }

        /// <summary>The three list-header sentinels that must count as "no folder assigned".</summary>
        private static readonly string[] HeaderSentinels =
        {
            "======= SEARCH RESULTS =======",
            "======= RECENT SELECTIONS ========",
            "========= SUGGESTIONS =========",
        };

        private const string NotAssignedReason =
            "a group whose SelectedFolder is null or is one of the three list-header sentinels has "
            + "no real destination, so the collection is not ready to move";

        /// <summary>
        /// Builds an item group whose controller reports <paramref name="selectedFolder"/> and
        /// carries just enough mocked mail detail for the notification text to be built.
        /// </summary>
        private static QfcItemGroup GroupWithFolder(int itemNumber, string selectedFolder)
        {
            var mail = new Mock<Outlook.MailItem>(MockBehavior.Loose);
            mail.SetupGet(m => m.SentOn).Returns(new DateTime(2026, 1, 1));
            mail.SetupGet(m => m.Subject).Returns("Subject " + itemNumber);
            var item = new Mock<IQfcItemController>(MockBehavior.Loose);
            item.SetupGet(i => i.SelectedFolder).Returns(selectedFolder);
            item.SetupGet(i => i.ItemNumber).Returns(itemNumber);
            item.SetupGet(i => i.Mail).Returns(mail.Object);
            return new QfcItemGroup { ItemController = item.Object };
        }

        /// <summary>
        /// Issue #474 defect 2. Readiness must be inspectable without presenting a dialog. One
        /// group has a null destination and one group carries each of the three header sentinels,
        /// so all four "not assigned" shapes are covered. A recording delegate replaces the modal
        /// notification, so no dialog is presented: before the seam the only way to evaluate
        /// readiness was to read the property, which showed a modal a unit test cannot dismiss.
        /// </summary>
        [TestMethod]
        public void TryGetMoveReadiness_WithUnassignedDestination_ReturnsFalseAndProducesNotificationText()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            var groups = new List<QfcItemGroup> { GroupWithFolder(1, null) };
            for (int i = 0; i < HeaderSentinels.Length; i++)
            {
                groups.Add(GroupWithFolder(i + 2, HeaderSentinels[i]));
            }
            QfcCollectionControllerTestSupport.SetField(controller, "_itemGroups", groups);
            string captured = null;
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_notifyNotReady",
                (Action<string>)(text => captured = text)
            );

            // Act
            bool ready = controller.TryGetMoveReadiness(out string notifications);
            bool readyViaProperty = controller.ReadyForMove;

            // Assert
            ready.Should().BeFalse(because: NotAssignedReason);
            readyViaProperty.Should().BeFalse(because: NotAssignedReason);
            notifications
                .Should()
                .StartWith(
                    "Can't complete actions! Not all emails assigned to folder",
                    because: "the notification opens with the fixed banner"
                )
                .And.ContainAll("Subject 1", "Subject 2", "Subject 3", "Subject 4");
            captured
                .Should()
                .Be(
                    notifications,
                    because: "the getter must hand the predicate's text to the notification "
                        + "delegate unchanged, which is what keeps production behaviour identical"
                );
        }

        /// <summary>
        /// Issue #474 defect 2. With every destination assigned, the predicate reports readiness
        /// and produces no notification text at all.
        /// </summary>
        [TestMethod]
        public void TryGetMoveReadiness_WithAllDestinationsAssigned_ReturnsTrueAndEmptyNotification()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_itemGroups",
                new List<QfcItemGroup> { GroupWithFolder(1, @"Inbox\Projects") }
            );

            // Act
            bool ready = controller.TryGetMoveReadiness(out string notifications);

            // Assert
            ready.Should().BeTrue(because: "every group has a real destination folder");
            notifications
                .Should()
                .BeEmpty(because: "there is nothing to notify when the collection is ready");
        }
    }
}
