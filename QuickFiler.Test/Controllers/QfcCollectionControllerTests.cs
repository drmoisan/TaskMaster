using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for QfcCollectionController focused on null-safety guards.
    /// QfcCollectionController requires WinForms UI components in its constructor, so
    /// instances are created via FormatterServices.GetUninitializedObject to bypass the
    /// constructor; all required private fields are then injected via reflection.
    /// </summary>
    [TestClass]
    public partial class QfcCollectionControllerTests
    {
        /// <summary>
        /// Creates an uninitialized QfcCollectionController with only the fields required
        /// for GetMoveDiagnostics set: _itemGroupsToMove is populated with one mocked entry.
        /// </summary>
        private static QfcCollectionController CreateControllerWithOneGroup(
            out Mock<IQfcItemController> mockItemController,
            out Mock<MailItemHelper> mockHelper
        )
        {
            // Use uninitialized object to bypass the WinForms-dependent constructor.
            var controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));

            // Build a minimal QfcItemGroup with a mocked IQfcItemController and MailItemHelper.
            mockHelper = new Mock<MailItemHelper>(MockBehavior.Loose);
            mockHelper.SetupGet(x => x.Subject).Returns("Test Subject");
            mockHelper.SetupGet(x => x.SenderName).Returns("Sender");
            mockHelper.SetupGet(x => x.ToRecipientsName).Returns("Recipient");
            mockHelper.SetupGet(x => x.SentDate).Returns(new DateTime(2026, 1, 1));

            mockItemController = new Mock<IQfcItemController>(MockBehavior.Loose);
            mockItemController.SetupGet(x => x.ItemHelper).Returns(mockHelper.Object);
            mockItemController.SetupGet(x => x.SelectedFolder).Returns("Inbox");

            var itemGroup = new QfcItemGroup();
            typeof(QfcItemGroup)
                .GetProperty(
                    nameof(QfcItemGroup.ItemController),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                ?.SetValue(itemGroup, mockItemController.Object);

            // If the property setter is internal, try the backing field directly.
            if (itemGroup.ItemController is null)
            {
                typeof(QfcItemGroup)
                    .GetField("_itemController", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(itemGroup, mockItemController.Object);
            }

            // Issue #469 defect 3: _itemGroupsToMove is now an ordered list, not a dictionary.
            var groupsToMove = new List<QfcItemGroup> { itemGroup };

            typeof(QfcCollectionController)
                .GetField("_itemGroupsToMove", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, groupsToMove);

            return controller;
        }

        /// <summary>
        /// Regression test for Issue #97: GetMoveDiagnostics must not throw a
        /// NullReferenceException when the olAppointment ref parameter is null.
        /// Before the fix, accessing olAppointment.Body at line 2115 throws NRE.
        /// After the fix, the null appointment is skipped gracefully.
        /// </summary>
        [TestMethod]
        public void GetMoveDiagnostics_WhenAppointmentIsNull_DoesNotThrow()
        {
            // Arrange
            var controller = CreateControllerWithOneGroup(
                out Mock<IQfcItemController> _,
                out Mock<MailItemHelper> _
            );
            AppointmentItem nullAppointment = null;

            // Act & Assert — must not throw NullReferenceException.
            // Before the fix, olAppointment.Body throws because olAppointment is null.
            System.Action act = () =>
                controller.GetMoveDiagnostics(
                    durationText: "5",
                    durationMinutesText: "0.08",
                    duration: 5.0,
                    dataLineBeg: "01/01/2026,12:00,",
                    endTime: new DateTime(2026, 1, 1, 12, 0, 0),
                    olAppointment: ref nullAppointment
                );
            act.Should().NotThrow();
        }

        /// <summary>
        /// Positive path: GetMoveDiagnostics must return a non-null string array
        /// when all inputs are valid, confirming non-null appointment path is also handled.
        /// </summary>
        [TestMethod]
        public void GetMoveDiagnostics_WhenAppointmentIsNull_ReturnsStringArray()
        {
            // Arrange
            var controller = CreateControllerWithOneGroup(
                out Mock<IQfcItemController> _,
                out Mock<MailItemHelper> _
            );
            AppointmentItem nullAppointment = null;

            // Act
            var result = controller.GetMoveDiagnostics(
                durationText: "5",
                durationMinutesText: "0.08",
                duration: 5.0,
                dataLineBeg: "01/01/2026,12:00,",
                endTime: new DateTime(2026, 1, 1, 12, 0, 0),
                olAppointment: ref nullAppointment
            );

            // Assert
            result.Should().NotBeNull();
        }

        // ---- RemoveBelowThresholdAsync (Issue #169) ----

        /// <summary>
        /// Builds an uninitialized QfcCollectionController whose <c>_itemGroups</c> field holds the
        /// supplied (entryId, topFolderScore) groups, and injects the removal seam
        /// (<c>_removeGroupByEntryId</c>) with a delegate that records the EntryIDs it is asked to
        /// remove. This isolates the below-threshold selection logic from all WinForms/COM state.
        /// </summary>
        private static QfcCollectionController CreateControllerWithGroups(
            IEnumerable<(string EntryId, long TopFolderScore)> groups,
            out List<string> removedEntryIds
        )
        {
            var controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));

            var itemGroups = new List<QfcItemGroup>();
            foreach (var (entryId, score) in groups)
            {
                var mail = new Mock<MailItem>(MockBehavior.Loose);
                mail.SetupGet(x => x.EntryID).Returns(entryId);

                var itemController = new Mock<IQfcItemController>(MockBehavior.Loose);
                itemController.SetupGet(x => x.TopFolderScore).Returns(score);

                var group = new QfcItemGroup { MailItem = mail.Object };
                typeof(QfcItemGroup)
                    .GetField("_itemController", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(group, itemController.Object);

                itemGroups.Add(group);
            }

            typeof(QfcCollectionController)
                .GetField("_itemGroups", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, itemGroups);

            var recorded = new List<string>();
            removedEntryIds = recorded;
            Func<string, Task> recordingRemoval = entryId =>
            {
                recorded.Add(entryId);
                return Task.CompletedTask;
            };
            typeof(QfcCollectionController)
                .GetField("_removeGroupByEntryId", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, recordingRemoval);

            return controller;
        }

        [TestMethod]
        public async Task RemoveBelowThresholdAsync_WhenAllGroupsAboveThreshold_RemovesNone()
        {
            // Arrange: threshold 0.9 -> cutoff 900; all groups score above 900.
            var controller = CreateControllerWithGroups(
                new[] { ("a", 950L), ("b", 1000L), ("c", 920L) },
                out var removed
            );

            // Act
            await controller.RemoveBelowThresholdAsync(0.9);

            // Assert
            removed.Should().BeEmpty();
        }

        [TestMethod]
        public async Task RemoveBelowThresholdAsync_WhenAllGroupsBelowThreshold_RemovesAll()
        {
            // Arrange: threshold 0.9 -> cutoff 900; all groups score below 900.
            var controller = CreateControllerWithGroups(
                new[] { ("a", 100L), ("b", 500L), ("c", 899L) },
                out var removed
            );

            // Act
            await controller.RemoveBelowThresholdAsync(0.9);

            // Assert
            removed.Should().BeEquivalentTo(new[] { "a", "b", "c" });
        }

        [TestMethod]
        public async Task RemoveBelowThresholdAsync_WhenMixed_RemovesOnlyBelowThresholdGroups()
        {
            // Arrange: threshold 0.9 -> cutoff 900.
            var controller = CreateControllerWithGroups(
                new[] { ("keepHigh", 950L), ("dropLow", 200L), ("keepEqualish", 901L) },
                out var removed
            );

            // Act
            await controller.RemoveBelowThresholdAsync(0.9);

            // Assert
            removed.Should().Equal("dropLow");
        }

        [TestMethod]
        public async Task RemoveBelowThresholdAsync_WhenScoreEqualsCutoff_RetainsGroup()
        {
            // Arrange: threshold 0.9 -> cutoff 900; one group scores exactly 900 (inclusive).
            var controller = CreateControllerWithGroups(
                new[] { ("boundary", 900L), ("below", 899L) },
                out var removed
            );

            // Act
            await controller.RemoveBelowThresholdAsync(0.9);

            // Assert
            removed.Should().Equal("below");
            removed.Should().NotContain("boundary");
        }

        [TestMethod]
        public async Task RemoveBelowThresholdAsync_WhenItemGroupsIsNull_DoesNothing()
        {
            // Arrange: uninitialized controller with a null _itemGroups field.
            var controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
            var recorded = new List<string>();
            Func<string, Task> recordingRemoval = entryId =>
            {
                recorded.Add(entryId);
                return Task.CompletedTask;
            };
            typeof(QfcCollectionController)
                .GetField("_removeGroupByEntryId", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, recordingRemoval);

            // Act & Assert: the null guard returns early without throwing or removing anything.
            Func<Task> act = () => controller.RemoveBelowThresholdAsync(0.9);

            await act.Should().NotThrowAsync();
            recorded.Should().BeEmpty();
        }

        [TestMethod]
        public async Task RemoveBelowThresholdAsync_WhenScoreIsZeroAndThresholdPositive_RemovesGroup()
        {
            // Arrange: a group with no qualifying suggestion (score 0) must be removed when the
            // cutoff is greater than 0.
            var controller = CreateControllerWithGroups(
                new[] { ("noSuggestion", 0L), ("strong", 980L) },
                out var removed
            );

            // Act
            await controller.RemoveBelowThresholdAsync(0.9);

            // Assert
            removed.Should().Equal("noSuggestion");
        }

        // Carrier-list carry tests (Issue #171, extended for #678) live in the partial part
        // QfcCollectionControllerTests.Part2.cs; see that file for the reason.

        // ---- Navigation-key register/unregister on page swap (Issue #232) ----

        /// <summary>
        /// Builds an uninitialized controller wired to exercise the navigation-key swap without live
        /// WinForms/COM state: a real KbdActions behind a Loose IQfcKeyboardHandler, Loose
        /// IEmailMoveMonitor and IQfcFormViewer (L1v0L2L3v_TableLayout returns null), the outgoing
        /// _itemGroups page, and _digits pre-set to 1. _digits = 1 is required because
        /// GetUninitializedObject bypasses the field initializer; without it the Digits getter would
        /// flip _digitRefreshNeeded and RegisterNavigation would enter the WinForms-bound SetVisualDigits path.
        /// </summary>
        private static QfcCollectionController CreateControllerForSwap(
            int outgoingItemCount,
            out KbdActions<string, KaStringAsync, Func<string, Task>> kbd
        )
        {
            var controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));

            var kbdActions = new KbdActions<string, KaStringAsync, Func<string, Task>>();
            kbd = kbdActions;
            var kbdHandler = new Mock<IQfcKeyboardHandler>(MockBehavior.Loose);
            kbdHandler.SetupGet(x => x.StringActionsAsync).Returns(() => kbdActions);

            var moveMonitor = new Mock<IEmailMoveMonitor>(MockBehavior.Loose);

            var formViewer = new Mock<IQfcFormViewer>(MockBehavior.Loose);
            formViewer
                .SetupGet(x => x.L1v0L2L3v_TableLayout)
                .Returns((System.Windows.Forms.TableLayoutPanel)null);

            SetControllerField(controller, "_kbdHandler", kbdHandler.Object);
            SetControllerField(controller, "_moveMonitor", moveMonitor.Object);
            SetControllerField(controller, "_formViewer", formViewer.Object);
            SetControllerField(controller, "_digits", 1);
            SetControllerField(controller, "_itemGroups", MakeGroups(outgoingItemCount));

            return controller;
        }

        /// <summary>Builds a list of <c>count</c> minimal item groups, each carrying a mock mail item.</summary>
        private static List<QfcItemGroup> MakeGroups(int count)
        {
            var groups = new List<QfcItemGroup>();
            for (int i = 0; i < count; i++)
            {
                var mail = new Mock<MailItem>(MockBehavior.Loose);
                mail.SetupGet(x => x.EntryID).Returns($"entry-{i}");
                groups.Add(new QfcItemGroup { MailItem = mail.Object });
            }
            return groups;
        }

        private static void SetControllerField(object target, string name, object value) =>
            typeof(QfcCollectionController)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(target, value);

        /// <summary>Seeds one <c>"Collection"</c>-sourced entry for <paramref name="key"/>.</summary>
        private static void SeedCollectionKey(
            KbdActions<string, KaStringAsync, Func<string, Task>> kbd,
            string key
        ) => kbd.Add("Collection", key, _ => Task.CompletedTask);

        /// <summary>Counts the <c>"Collection"</c>-sourced entries registered for <paramref name="key"/>.</summary>
        private static int CountCollectionKey(
            KbdActions<string, KaStringAsync, Func<string, Task>> kbd,
            string key
        ) => kbd.Count(a => a.SourceId == "Collection" && a.Key == key);

        /// <summary>
        /// [P1-T1] Reported reproduction (Issue #232). A 1-item outgoing page has its navigation key
        /// "1" registered plus an orphaned "2" left behind by an earlier page abandoned through the
        /// pre-fix defective swap path. Swapping in a cached 2-item page walks keys "1" and "2", so
        /// "2" collides with the orphan. Pre-fix, <c>LoadControlsAndHandlers_01</c> performs no
        /// navigation registration at all, so the call does not throw and this expect-fail assertion
        /// fails (the collision is not reproduced at this call boundary). Post-fix the swap routes
        /// through <c>SwapItemGroups</c>, which now unregisters the outgoing page and re-registers the
        /// incoming page; adding key "2" surfaces the documented <see cref="ArgumentException"/>,
        /// proving navigation registration now occurs during the swap.
        /// </summary>
        [TestMethod]
        public void LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix()
        {
            // Arrange
            var controller = CreateControllerForSwap(outgoingItemCount: 1, out var kbd);
            controller.RegisterNavigation();
            SeedCollectionKey(kbd, "2");
            var cachedTwoItemPage = MakeGroups(2);

            // Act
            System.Action act = () =>
                controller.LoadControlsAndHandlers_01(null, cachedTwoItemPage);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Key 2 SourceId Collection*");
        }

        /// <summary>
        /// [P3-T1] (AC1) A page swap unregisters every outgoing "Collection" key and registers exactly
        /// the incoming page's keys.
        /// </summary>
        [TestMethod]
        public void LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys()
        {
            // Arrange: 2-item outgoing page with keys "1" and "2" registered.
            var controller = CreateControllerForSwap(outgoingItemCount: 2, out var kbd);
            controller.RegisterNavigation();
            var oneItemIncomingPage = MakeGroups(1);

            // Act
            controller.LoadControlsAndHandlers_01(null, oneItemIncomingPage);

            // Assert: no stale outgoing key remains; exactly one incoming key "1".
            CountCollectionKey(kbd, "2").Should().Be(0);
            CountCollectionKey(kbd, "1").Should().Be(1);
            kbd.Count(a => a.SourceId == "Collection").Should().Be(1);
        }

        /// <summary>
        /// [P3-T3] KbdActions.Add throws on a duplicate key; registering the same page twice without an
        /// intervening unregister triggers the collision the Phase 2 guard exists to avoid.
        /// </summary>
        [TestMethod]
        public void RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException()
        {
            // Arrange: 2-item page, no keys registered yet.
            var controller = CreateControllerForSwap(outgoingItemCount: 2, out _);

            // Act: first registration succeeds; the second re-adds the same keys.
            controller.RegisterNavigation();
            System.Action secondRegister = () => controller.RegisterNavigation();

            // Assert
            secondRegister
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("*SourceId Collection*");
        }

        /// <summary>
        /// [P3-T4] (AC3) The guarded zero-item flow (unregister outgoing, drop its item, then swap in a
        /// cached page) leaves exactly one "Collection" entry per incoming key and throws nothing,
        /// confirming the production guard's effect (skipping the redundant trailing register).
        /// </summary>
        [TestMethod]
        public void SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey()
        {
            // Arrange: 1-item outgoing page with key "1" registered.
            var controller = CreateControllerForSwap(outgoingItemCount: 1, out var kbd);
            controller.RegisterNavigation();
            var twoItemCachedPage = MakeGroups(2);

            // Act: unregister the outgoing page, drop its item, then swap in the cached page.
            controller.UnregisterNavigation();
            GetItemGroups(controller).RemoveAt(0);
            System.Action act = () =>
                controller.LoadControlsAndHandlers_01(null, twoItemCachedPage);

            // Assert: no exception; exactly one entry per incoming key, no duplicates.
            act.Should().NotThrow();
            CountCollectionKey(kbd, "1").Should().Be(1);
            CountCollectionKey(kbd, "2").Should().Be(1);
            kbd.Count(a => a.SourceId == "Collection").Should().Be(2);
        }

        private static List<QfcItemGroup> GetItemGroups(QfcCollectionController controller) =>
            (List<QfcItemGroup>)
                typeof(QfcCollectionController)
                    .GetField("_itemGroups", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(controller);
    }
}
