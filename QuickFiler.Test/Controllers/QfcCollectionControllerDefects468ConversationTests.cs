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
    /// <c>ToggleUnGroupConv</c> itself cannot be driven COM-free: its first two statements are
    /// <c>SafeSetTlpLayout(false)</c> and <c>UnregisterNavigation()</c>, and <c>MakeSpaceForItems</c>
    /// reaches <c>TableLayoutHelper.InsertSpecificRow</c> on the WinForms item panel. The
    /// reconciliation contract is therefore asserted against the pure static helpers the fix
    /// extracts, per decision D7 of the plan. The behavioural pre-fix red states that have no
    /// permanent post-fix counterpart at the <c>ToggleUnGroupConv</c> level are recorded in the
    /// fail-before dossier with that reason.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcCollectionControllerDefects468ConversationTests
    {
        private const BindingFlags AnyStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Issue #470 defect 2. Structural test asserting that the two pure static reconciliation
        /// helpers exist on <see cref="QfcCollectionController"/>.
        /// <para>
        /// Scenario: look both members up by reflection. Expected outcome: both are found and both
        /// are static. Before the fix neither exists, because the member-resolution expression is
        /// inline in <c>EnumerateConversationMembers</c> and the count disagreement is never
        /// detected at all.
        /// </para>
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
            resolve
                .Should()
                .NotBeNull(
                    because: "issue #470 defect 2 extracts the conversation member-resolution "
                        + "expression into a pure static helper so it can be resolved once, before "
                        + "MakeSpaceForItems, instead of being re-resolved inside the loop"
                );
            resolve
                .IsStatic.Should()
                .BeTrue(because: "the helper must not touch controller instance state");
            reconcile
                .Should()
                .NotBeNull(
                    because: "issue #470 defect 2 requires a single source of truth for the "
                        + "insertion count, and a pure helper is the only part of that path that "
                        + "can be asserted without COM"
                );
            reconcile
                .IsStatic.Should()
                .BeTrue(because: "the helper must not touch controller instance state");
        }

        /// <summary>
        /// Issue #470 defect 2. Contract test for the extracted member-resolution helper.
        /// <para>
        /// Scenario: a resolver whose same-folder conversation items are the base email plus three
        /// others with distinct sent times, injected through the public <c>ConversationItems</c>
        /// setter. Expected outcome: the base entry is excluded and the remainder is ordered by
        /// sent time descending.
        /// </para>
        /// <para>
        /// The ordering is not incidental. <c>EnumerateConversationMembers</c> assigns
        /// <c>insertions[i]</c> to the group at <c>insertionIndex + i</c>, so the order of this list
        /// is the on-screen order of the expanded conversation.
        /// </para>
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
            entryIds
                .Should()
                .NotContain(
                    "base",
                    because: "the base email already occupies its own row; re-inserting it would "
                        + "duplicate the message the expansion hangs beneath"
                );
            entryIds
                .Should()
                .HaveCount(
                    3,
                    because: "three of the four conversation items are members other than the base"
                );
            entryIds.Should().Equal(new[] { "newest", "middle", "oldest" });
        }

        /// <summary>
        /// Issue #470 defect 2, above-reservation case. More members resolved than the caller
        /// reserved rows for.
        /// <para>
        /// Scenario: the caller reserves <c>conversationCount - 1 == 2</c> rows while four members
        /// resolve. Expected outcome: the resolved count of four is returned as the single source of
        /// truth, and the warning delegate is invoked exactly once with a message naming all six
        /// values.
        /// </para>
        /// <para>
        /// This is the case that previously threw <c>ArgumentOutOfRangeException</c> inside
        /// <c>EnumerateConversationMembers</c>, because the loop ran to the resolved count while only
        /// the reserved rows existed. Per decision D5 the production behaviour is log-and-proceed,
        /// not throw: this member sits on the VSTO UI event path and the state is recoverable.
        /// </para>
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
                .Be(
                    4,
                    because: "the resolved member list is the only count that describes the rows "
                        + "actually about to be written, so it is the single source of truth"
                );
            sink.Messages.Should()
                .HaveCount(
                    1,
                    because: "one disagreement must produce exactly one log entry, not one per "
                        + "surplus member"
                );
            sink.Messages[0].Should().Contain("entryID=entry-7");
            sink.Messages[0].Should().Contain("conversationCount=3");
            sink.Messages[0].Should().Contain("insertionsCount=4");
            sink.Messages[0].Should().Contain("sameFolderCount=5");
            sink.Messages[0].Should().Contain("expandedCount=9");
            sink.Messages[0].Should().Contain("baseEmailIndex=2");
        }

        /// <summary>
        /// Issue #470 defect 2, equal case. The reservation and the resolved count agree.
        /// <para>
        /// Scenario: the caller reserves <c>conversationCount - 1 == 2</c> rows and two members
        /// resolve. Expected outcome: two is returned and the warning delegate is never invoked.
        /// </para>
        /// <para>
        /// This is the normal path and it carries the negative half of the contract. Without it a
        /// reconciliation that warned unconditionally would still satisfy the two disagreement
        /// tests while flooding the log on every conversation expansion.
        /// </para>
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
            reconciled
                .Should()
                .Be(
                    2,
                    because: "the return value is the resolved count in every case, agreement "
                        + "included"
                );
            sink.Messages.Should()
                .BeEmpty(
                    because: "the counts agree, and a warning on the normal path would make the "
                        + "log useless for spotting the abnormal one"
                );
        }

        /// <summary>
        /// Issue #470 defect 2, below-reservation case. Fewer members resolved than the caller
        /// reserved rows for.
        /// <para>
        /// Scenario: the caller reserves <c>conversationCount - 1 == 4</c> rows while one member
        /// resolves. Expected outcome: one is returned, and the warning delegate is invoked exactly
        /// once.
        /// </para>
        /// <para>
        /// This direction is the quieter defect: no exception is raised, the surplus reserved rows
        /// are simply left as empty item groups in the collection. Returning the resolved count is
        /// what stops those rows being reserved in the first place.
        /// </para>
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
                .Be(
                    1,
                    because: "reserving four rows for one member leaves three empty groups; the "
                        + "resolved count is what the caller must reserve against"
                );
            sink.Messages.Should()
                .HaveCount(
                    1,
                    because: "a shortfall is as much a snapshot disagreement as a surplus and must "
                        + "be logged once"
                );
            sink.Messages[0].Should().Contain("insertionsCount=1");
            sink.Messages[0].Should().Contain("conversationCount=5");
        }

        /// <summary>
        /// Issue #470 defect 2. Proves the retyped <c>EnumerateConversationMembers</c> consumes the
        /// caller-supplied list and performs no resolver query of its own.
        /// <para>
        /// Scenario: an uninitialized controller and an empty insertions list, so the body's single
        /// loop executes zero iterations and no COM-bound group initialization runs. Expected
        /// outcome: no exception.
        /// </para>
        /// <para>
        /// This is the only assertion the method admits without COM. Every statement inside the loop
        /// reaches <c>_itemGroups</c>, <c>LoadItemViewer_03</c> or a live
        /// <c>QfcItemController</c>. With the pre-fix signature the method would still have queried
        /// <c>resolver.ConversationItems</c> before the loop and produced a non-empty list from the
        /// injected snapshot, so an empty-list arrangement was not expressible at all: the argument
        /// did not exist. That the method now runs to completion on an empty caller-supplied list is
        /// the observable proof that the resolver query is gone.
        /// </para>
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
            act.Should()
                .NotThrow(
                    because: "the method now iterates the caller-supplied list and nothing else, so "
                        + "an empty list is a complete, side-effect-free execution"
                );
        }

        /// <summary>
        /// Builds a mocked <see cref="MailItem"/> carrying only the two members the resolution
        /// helper reads.
        /// </summary>
        private static MailItem BuildMailItem(string entryId, DateTime sentOn)
        {
            Mock<MailItem> mailItem = new Mock<MailItem>(MockBehavior.Loose);
            mailItem.SetupGet(item => item.EntryID).Returns(entryId);
            mailItem.SetupGet(item => item.SentOn).Returns(sentOn);
            return mailItem.Object;
        }

        /// <summary>
        /// Builds a <see cref="ConversationResolver"/> whose conversation items are injected through
        /// the public setter, with no COM, no dataframe load and no globals.
        /// </summary>
        /// <remarks>
        /// The mail item passed to the constructor must be non-null. The <c>ConversationItems</c>
        /// getter routes through <c>Initializer.GetOrLoad(ref field, loader, callback, strict,
        /// dependencies)</c> with the resolver's own mail item as the dependency, and that overload
        /// returns <c>default</c> — a pair of nulls — rather than the injected value when any
        /// dependency is null. A null mail item would therefore silently discard the injection.
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
