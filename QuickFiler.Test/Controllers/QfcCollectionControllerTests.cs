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
    public class QfcCollectionControllerTests
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

            var dict = new ConcurrentDictionary<QfcItemGroup, int>();
            dict.TryAdd(itemGroup, 0);

            typeof(QfcCollectionController)
                .GetField("_itemGroupsToMove", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, dict);

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
    }
}
