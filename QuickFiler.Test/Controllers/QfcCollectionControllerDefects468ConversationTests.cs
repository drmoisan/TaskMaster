using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for the conversation-path defects in the issue #468 family: issue #470
    /// defects 1, 2 and 3. None of these tests needs COM, a live Outlook, a WinForms control, or an
    /// STA apartment.
    /// <para>
    /// <c>ToggleUnGroupConv</c> cannot be driven COM-free, so per decision D7 the reconciliation
    /// contract is asserted against the pure static helpers the fix extracts. The behavioural
    /// pre-fix states with no permanent post-fix counterpart are recorded, with that reason, in
    /// the fail-before dossier and in the P7-T12 evidence artifact.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcCollectionControllerDefects468ConversationTests
    {
        private const BindingFlags AnyStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Issue #470 defect 2. Structural test: both pure reconciliation helpers exist and are
        /// static. Before the fix neither exists, because the member-resolution expression is inline
        /// in <c>EnumerateConversationMembers</c> and the count disagreement is never detected.
        /// </summary>
        [TestMethod]
        public void ConversationReconciliationHelpersExist()
        {
            // Arrange
            Type controller = typeof(QfcCollectionController);

            // Act
            MethodInfo resolve = controller.GetMethod("ResolveConversationInsertions", AnyStatic);
            MethodInfo reconcile = controller.GetMethod("ReconcileInsertionCount", AnyStatic);

            // Assert
            resolve.Should().NotBeNull(because: "the members must be resolved once, up front");
            resolve.IsStatic.Should().BeTrue(because: "the helper must touch no instance state");
            reconcile.Should().NotBeNull(because: "the insertion count needs one source of truth");
            reconcile.IsStatic.Should().BeTrue(because: "the helper must touch no instance state");
        }

        /// <summary>
        /// Issue #470 defect 2. The extracted resolution helper excludes the base entry and orders
        /// the remainder newest first. The order is the on-screen order, because
        /// <c>EnumerateConversationMembers</c> writes <c>insertions[i]</c> to
        /// <c>insertionIndex + i</c>.
        /// </summary>
        [TestMethod]
        public void ResolveConversationInsertions_ExcludesBaseEntryAndOrdersBySentOnDescending()
        {
            // Arrange
            IList<MailItem> items = new List<MailItem>
            {
                BuildMailItem("base", new DateTime(2026, 1, 4)),
                BuildMailItem("oldest", new DateTime(2026, 1, 1)),
                BuildMailItem("newest", new DateTime(2026, 1, 3)),
                BuildMailItem("middle", new DateTime(2026, 1, 2)),
            };
            ConversationResolver resolver = BuildResolverWithConversationItems(items);

            // Act
            IReadOnlyList<MailItem> insertions =
                QfcCollectionController.ResolveConversationInsertions(resolver, "base");

            // Assert
            List<string> entryIds = insertions.Select(mailItem => mailItem.EntryID).ToList();
            entryIds.Should().NotContain("base", because: "the base email keeps its existing row");
            entryIds.Should().HaveCount(3, because: "three of the four items are not the base");
            entryIds.Should().Equal(new[] { "newest", "middle", "oldest" });
        }

        /// <summary>
        /// Issue #470 defect 2, above-reservation case: four members resolve while the caller
        /// reserved two rows. The resolved count wins and one warning is emitted. This is the case
        /// that previously threw <c>ArgumentOutOfRangeException</c> from the insertion loop; per D5
        /// the production response is log-and-proceed, not throw.
        /// </summary>
        [TestMethod]
        public void ReconcileInsertionCount_AboveReservation_ReturnsInsertionsCountAndWarnsOnce()
        {
            // Arrange
            WarningSink sink = new WarningSink();

            // Act
            int reconciled = QfcCollectionController.ReconcileInsertionCount(
                entryID: "entry-7",
                conversationCount: 3,
                insertionsCount: 4,
                sameFolderCount: 5,
                expandedCount: 9,
                baseEmailIndex: 2,
                warn: sink.Accept
            );

            // Assert
            reconciled
                .Should()
                .Be(4, because: "only the resolved list describes the rows written");
            sink.Messages.Should().HaveCount(1, because: "one disagreement means one log entry");
            sink.Messages[0].Should().Contain("entryID=entry-7");
            sink.Messages[0].Should().Contain("conversationCount=3");
            sink.Messages[0].Should().Contain("insertionsCount=4");
            sink.Messages[0].Should().Contain("sameFolderCount=5");
            sink.Messages[0].Should().Contain("expandedCount=9");
            sink.Messages[0].Should().Contain("baseEmailIndex=2");
        }

        /// <summary>
        /// Issue #470 defect 2, equal case: the reservation and the resolved count agree, so no
        /// warning is emitted. This carries the negative half of the contract; without it a helper
        /// that warned unconditionally would still satisfy both disagreement tests.
        /// </summary>
        [TestMethod]
        public void ReconcileInsertionCount_EqualToReservation_ReturnsInsertionsCountAndDoesNotWarn()
        {
            // Arrange
            WarningSink sink = new WarningSink();

            // Act
            int reconciled = QfcCollectionController.ReconcileInsertionCount(
                entryID: "entry-7",
                conversationCount: 3,
                insertionsCount: 2,
                sameFolderCount: 3,
                expandedCount: 3,
                baseEmailIndex: 2,
                warn: sink.Accept
            );

            // Assert
            reconciled.Should().Be(2, because: "the resolved count is returned in every case");
            sink.Messages.Should()
                .BeEmpty(because: "warning on the normal path buries the abnormal");
        }

        /// <summary>
        /// Issue #470 defect 2, below-reservation case: one member resolves while the caller
        /// reserved four rows. The quieter direction of the defect, which raises nothing and simply
        /// leaves surplus empty item groups behind.
        /// </summary>
        [TestMethod]
        public void ReconcileInsertionCount_BelowReservation_ReturnsInsertionsCountAndWarnsOnce()
        {
            // Arrange
            WarningSink sink = new WarningSink();

            // Act
            int reconciled = QfcCollectionController.ReconcileInsertionCount(
                entryID: "entry-7",
                conversationCount: 5,
                insertionsCount: 1,
                sameFolderCount: 2,
                expandedCount: 6,
                baseEmailIndex: 0,
                warn: sink.Accept
            );

            // Assert
            reconciled
                .Should()
                .Be(1, because: "four reserved rows for one member leaves three empty");
            sink.Messages.Should().HaveCount(1, because: "a shortfall is a disagreement too");
            sink.Messages[0].Should().Contain("insertionsCount=1");
            sink.Messages[0].Should().Contain("conversationCount=5");
        }

        /// <summary>
        /// Issue #470 defect 2. The retyped <c>EnumerateConversationMembers</c> consumes the
        /// caller-supplied list and issues no resolver query. An empty list executes zero loop
        /// iterations, so nothing COM-bound runs. With the pre-fix signature this arrangement was
        /// not expressible: the argument did not exist and the method re-resolved its own list.
        /// </summary>
        [TestMethod]
        public void EnumerateConversationMembers_WithNoInsertions_DoesNotThrow()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            ConversationResolver resolver = BuildResolverWithConversationItems(
                new List<MailItem> { BuildMailItem("base", new DateTime(2026, 1, 1)) }
            );

            // Act
            System.Action act = () =>
                controller.EnumerateConversationMembers(
                    "base",
                    resolver,
                    insertionIndex: 1,
                    insertions: Array.Empty<MailItem>(),
                    folderList: null
                );

            // Assert
            act.Should().NotThrow(because: "an empty list is a complete, side-effect-free run");
        }

        /// <summary>
        /// Issue #470 defect 1. A promotion request that matches no child returns the sentinel
        /// <c>-1</c> and leaves the caller's child count alone. Before the fix the method evaluated
        /// <c>_itemGroups[indexOriginal].ItemViewer</c> immediately after the lookup, so a miss
        /// subscripted with <c>-1</c> and threw onto the VSTO UI thread. Per D4 the contract is a
        /// sentinel return, not a typed throw, because the caller can recover.
        /// </summary>
        [TestMethod]
        public void PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting()
        {
            // Arrange
            QfcCollectionController controller = BuildControllerWithGroups(
                BuildItemController(null, "other-1"),
                BuildItemController(null, "other-2")
            );
            int observedChildCount = int.MinValue;
            int promoted = int.MinValue;

            // Act
            System.Action act = () =>
            {
                int childCount = 2;
                promoted = controller.PromoteFirstChild("missing-original", ref childCount);
                observedChildCount = childCount;
            };

            // Assert
            act.Should().NotThrow(because: "a missing original must be handled, not subscripted");
            promoted.Should().Be(-1, because: "D4 fixes the contract as a sentinel return");
            observedChildCount
                .Should()
                .Be(2, because: "no child was promoted, so none was consumed");
        }

        /// <summary>
        /// Issue #470 defect 1, end to end through the string overload of <c>ToggleGroupConv</c>.
        /// No group carries the requested identifier as its own entry or as its conversation
        /// origin, so the child count is zero and the collapse branch is never entered. Before the
        /// fix the <c>-1</c> from the lookup reached <c>PromoteFirstChild</c> and then
        /// <c>ChangeConversationSilently(-1, true)</c>, either of which subscripts the group list
        /// with a negative index.
        /// </summary>
        [TestMethod]
        public void ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne()
        {
            // Arrange
            QfcCollectionController controller = BuildControllerWithGroups(
                BuildItemController(null, "other-1")
            );

            // Act
            System.Action act = () => controller.ToggleGroupConv("missing-original");

            // Assert
            act.Should()
                .NotThrow(
                    because: "a conversation whose original is gone must be a no-op on the UI "
                        + "event path, not an ArgumentOutOfRangeException raised into VSTO"
                );
        }

        /// <summary>
        /// Issue #470 defect 3. <c>SetVisualDigits</c> must skip a group whole when either its item
        /// controller or its item viewer is missing.
        /// <para>
        /// Two groups, both with a null <c>ItemViewer</c>: a default group whose controller is also
        /// null, and one carrying a mocked controller. The loaded-email guard still passes because
        /// it counts groups. Before the fix the first group throws
        /// <see cref="NullReferenceException"/> on <c>grp.ItemController.ItemNumberDigits</c>.
        /// </para>
        /// <para>
        /// The second group is what makes a controller-only guard visibly insufficient: with one,
        /// execution reaches <c>grp.ItemViewer.LblItemNumber</c> on the next line and throws on the
        /// same arrangement. Because every group's viewer is null here, any attempt to write viewer
        /// text would throw, so completing without an exception is itself the proof that none was
        /// written; <c>VerifySet</c> on the live controller confirms the group was skipped before
        /// the write rather than after it.
        /// </para>
        /// </summary>
        [TestMethod]
        public void SetVisualDigits_WithNullItemController_SkipsTheGroupWithoutThrowing()
        {
            // Arrange
            Mock<IQfcItemController> liveController = new Mock<IQfcItemController>(
                MockBehavior.Loose
            );
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_itemGroups",
                new List<QfcItemGroup>
                {
                    new QfcItemGroup(),
                    new QfcItemGroup { ItemController = liveController.Object },
                }
            );

            // Act
            System.Exception captured = null;
            try
            {
                QfcCollectionControllerTestSupport.InvokeNonPublic(
                    controller,
                    "SetVisualDigits",
                    1
                );
            }
            catch (TargetInvocationException wrapper)
            {
                // Reflection wraps the real failure; assert on the inner exception.
                captured = wrapper.InnerException;
            }

            // Assert
            captured
                .Should()
                .BeNull(
                    because: "issue #470 defect 3 requires a group with no controller or no viewer "
                        + "to be skipped rather than dereferenced"
                );
            QfcCollectionControllerTestSupport
                .GetField(controller, "_digitRefreshNeeded")
                .Should()
                .Be(false, because: "the method must reach its final statement, not abort midway");
            liveController.VerifySet(
                item => item.ItemNumberDigits = It.IsAny<int>(),
                Times.Never(),
                "a group whose viewer is null must be skipped whole; writing the controller first "
                    + "and only then failing on the viewer is the defect, not the fix"
            );
        }

        /// <summary>
        /// Builds a mocked <see cref="MailItem"/> carrying only the two members the conversation
        /// helpers read.
        /// </summary>
        private static MailItem BuildMailItem(string entryId, DateTime sentOn)
        {
            Mock<MailItem> mailItem = new Mock<MailItem>(MockBehavior.Loose);
            mailItem.SetupGet(item => item.EntryID).Returns(entryId);
            mailItem.SetupGet(item => item.SentOn).Returns(sentOn);
            return mailItem.Object;
        }

        /// <summary>
        /// Builds a mocked <see cref="IQfcItemController"/> reporting the supplied conversation
        /// origin identifier and a mail item with the supplied entry identifier.
        /// </summary>
        /// <remarks>
        /// Both members must be set up explicitly: a loose mock returns <see langword="null"/> for
        /// <c>Mail</c>, and the conversation lookups dereference <c>Mail.EntryID</c>, so an
        /// unconfigured mock would fail with <see cref="NullReferenceException"/> and mask the index
        /// defect these tests target.
        /// </remarks>
        private static IQfcItemController BuildItemController(string convOriginId, string entryId)
        {
            Mock<IQfcItemController> itemController = new Mock<IQfcItemController>(
                MockBehavior.Loose
            );
            itemController.SetupGet(item => item.ConvOriginID).Returns(convOriginId);
            itemController
                .SetupGet(item => item.Mail)
                .Returns(BuildMailItem(entryId, new DateTime(2026, 1, 1)));
            return itemController.Object;
        }

        /// <summary>
        /// Builds an uninitialized controller whose live group list holds one group per supplied
        /// item controller.
        /// </summary>
        private static QfcCollectionController BuildControllerWithGroups(
            params IQfcItemController[] itemControllers
        )
        {
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            List<QfcItemGroup> groups = itemControllers
                .Select(itemController => new QfcItemGroup { ItemController = itemController })
                .ToList();
            QfcCollectionControllerTestSupport.SetField(controller, "_itemGroups", groups);
            return controller;
        }

        /// <summary>
        /// Builds a <see cref="ConversationResolver"/> whose conversation items are injected through
        /// the public setter, with no COM, no dataframe load and no globals.
        /// </summary>
        /// <remarks>
        /// The mail item passed to the constructor must be non-null. The <c>ConversationItems</c>
        /// getter routes through the <c>Initializer.GetOrLoad</c> overload that takes dependencies,
        /// and that overload returns <c>default</c> — a pair of nulls — rather than the injected
        /// value when any dependency is null. A null mail item would silently discard the injection.
        /// </remarks>
        private static ConversationResolver BuildResolverWithConversationItems(
            IList<MailItem> items
        )
        {
            ConversationResolver resolver = new ConversationResolver(
                null,
                BuildMailItem("resolver-anchor", new DateTime(2026, 1, 1))
            );
            resolver.ConversationItems = new Pair<IList<MailItem>>(
                sameFolder: items,
                expanded: items
            );
            return resolver;
        }

        /// <summary>
        /// Collects the messages a reconciliation warning delegate receives, so a test can assert
        /// both the invocation count and the message content.
        /// </summary>
        private sealed class WarningSink
        {
            internal List<string> Messages { get; } = new List<string>();

            internal void Accept(string message)
            {
                Messages.Add(message);
            }
        }
    }
}
