using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for the move-path defects in the issue #468 family: issue #469 defects 1,
    /// 2, 3 and 4, and issue #473 defect 2. None of these tests needs COM, a live Outlook, a
    /// WinForms control, or an STA apartment.
    /// <para>
    /// A companion file, <c>QfcCollectionController.TestSupport.cs</c>, carries the shared asserting
    /// reflection helpers and the uninitialized-controller builder.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcCollectionControllerDefects468MoveTests
    {
        /// <summary>
        /// Issue #469 defect 3. Structural regression test asserting that the move collection is
        /// declared as an <em>ordered</em> contract.
        /// <para>
        /// Scenario: read the declared type of the private <c>_itemGroupsToMove</c> field. Expected
        /// outcome: it is assignable to <see cref="IReadOnlyList{T}"/> of <c>QfcItemGroup</c>.
        /// </para>
        /// <para>
        /// Before the fix the field is a <c>ConcurrentDictionary&lt;QfcItemGroup, int&gt;</c>, whose
        /// enumeration order is unspecified. <c>TryGetItemGroupByIndex</c> nevertheless resolves a
        /// group positionally with <c>ElementAt(index)</c>, and <c>MoveEmailsAsync</c> and
        /// <c>GetMoveDiagnostics</c> both drive that positional lookup, so a rehash can silently
        /// pair one message's move with another message's diagnostics. The declared type is the
        /// proof that a positional contract is now backed by an ordered collection.
        /// </para>
        /// </summary>
        [TestMethod]
        public void ItemGroupsToMoveFieldDeclaresAnOrderedContract()
        {
            // Arrange
            FieldInfo field = QfcCollectionControllerTestSupport.GetFieldInfo("_itemGroupsToMove");

            // Act
            Type declared = field.FieldType;

            // Assert
            declared
                .Should()
                .BeAssignableTo<IReadOnlyList<QfcItemGroup>>(
                    because: "issue #469 defect 3 requires the move collection to guarantee the "
                        + "insertion order that TryGetItemGroupByIndex, MoveEmailsAsync and "
                        + "GetMoveDiagnostics all depend on when they resolve a group by position"
                );
        }

        /// <summary>
        /// Issue #469 defect 3. Behavioural contract test for positional resolution after the live
        /// group list has been mutated.
        /// <para>
        /// Scenario: cache three groups, remove the middle one, append a fourth, then re-cache.
        /// Expected outcome: index <c>0</c>, <c>1</c> and <c>2</c> resolve to the first, third and
        /// fourth group in that order, and both an index of <c>-1</c> and an index equal to the
        /// count return <see langword="null"/> rather than throwing.
        /// </para>
        /// <para>
        /// This test has no deterministic pre-fix red state: a <c>ConcurrentDictionary</c>'s
        /// enumeration order is unspecified rather than guaranteed-wrong, so a pre-fix run could
        /// happen to return the right order and the assertion would be flaky by construction. The
        /// structural assertion in
        /// <see cref="ItemGroupsToMoveFieldDeclaresAnOrderedContract"/> carries the deterministic
        /// fail-before proof; this test carries the permanent post-fix behavioural contract. The
        /// exception is recorded in the fail-before dossier.
        /// </para>
        /// </summary>
        [TestMethod]
        public void TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            QfcItemGroup first = new QfcItemGroup();
            QfcItemGroup second = new QfcItemGroup();
            QfcItemGroup third = new QfcItemGroup();
            QfcItemGroup fourth = new QfcItemGroup();
            List<QfcItemGroup> groups = new List<QfcItemGroup> { first, second, third };
            QfcCollectionControllerTestSupport.SetField(controller, "_itemGroups", groups);
            controller.CacheItemGroupsForMove();

            groups.Remove(second);
            groups.Add(fourth);

            // Act
            controller.CacheItemGroupsForMove();

            // Assert
            ResolveByIndex(controller, 0)
                .Should()
                .BeSameAs(first, because: "the first group keeps position 0 after the mutation");
            ResolveByIndex(controller, 1)
                .Should()
                .BeSameAs(
                    third,
                    because: "removing the middle group promotes the third group to position 1"
                );
            ResolveByIndex(controller, 2)
                .Should()
                .BeSameAs(fourth, because: "the appended group takes the last position");
            ResolveByIndex(controller, -1)
                .Should()
                .BeNull(
                    because: "a negative index is outside the collection and must be reported as a "
                        + "missing group rather than throwing"
                );
            ResolveByIndex(controller, 3)
                .Should()
                .BeNull(
                    because: "an index equal to the count is one past the end and must be reported "
                        + "as a missing group rather than throwing"
                );
        }

        /// <summary>
        /// Issue #473 defect 2. Regression test proving that a cancellation raised while a message
        /// is being moved reaches the caller instead of being recorded as a move failure.
        /// <para>
        /// Scenario: one cached move group whose mocked item controller faults
        /// <c>MoveMailAsync()</c> with <see cref="OperationCanceledException"/>. Expected outcome:
        /// that exception propagates out of <c>MoveEmailsAsync</c>.
        /// </para>
        /// <para>
        /// Before the fix the broad <c>catch (System.Exception)</c> in
        /// <c>TryMoveEmailByGroupAsync</c> swallows it, logs it as an error, and lets the batch
        /// continue moving the remaining messages â€” the opposite of what a cancellation requests.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            Mock<IQfcItemController> itemController = new Mock<IQfcItemController>(
                MockBehavior.Loose
            );
            itemController
                .Setup(item => item.MoveMailAsync())
                .ThrowsAsync(new OperationCanceledException("the move batch was cancelled"));
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_itemGroupsToMove",
                new List<QfcItemGroup>
                {
                    new QfcItemGroup { ItemController = itemController.Object },
                }
            );

            // Act
            Func<Task> act = () => controller.MoveEmailsAsync(null);

            // Assert
            await act.Should()
                .ThrowAsync<OperationCanceledException>(
                    because: "issue #473 defect 2 requires cancellation to reach the caller so an "
                        + "aborted batch stops, instead of being swallowed by the broad catch and "
                        + "logged as a move error"
                );
        }

        /// <summary>
        /// Issue #473 defect 2. Regression test proving that one root failure produces one log
        /// entry rather than two.
        /// <para>
        /// Scenario: one cached move group whose <c>ItemController</c> is <see langword="null"/>
        /// and whose <c>MailItem</c> is a mock whose <c>Subject</c> getter throws. Expected
        /// outcome: <c>MoveEmailsAsync</c> completes without throwing, and <c>Subject</c> is never
        /// read.
        /// </para>
        /// <para>
        /// Before the fix the broad catch handled the first dereference and then fell through to
        /// <c>group.MailItem.Subject</c> inside that same catch, dereferencing a second time and
        /// raising a second exception into the nested catch, so a single root cause emitted two
        /// misleading <c>logger.Error</c> entries. Asserting <c>Times.Never()</c> on the
        /// <c>Subject</c> getter is the observable proof that the second dereference no longer
        /// happens.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime()
        {
            // Arrange
            Mock<MailItem> mailItem = new Mock<MailItem>(MockBehavior.Loose);
            mailItem
                .SetupGet(mail => mail.Subject)
                .Throws(
                    new InvalidOperationException("Subject is unavailable in this arrangement")
                );
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_itemGroupsToMove",
                new List<QfcItemGroup>
                {
                    new QfcItemGroup { ItemController = null, MailItem = mailItem.Object },
                }
            );

            // Act
            Func<Task> act = () => controller.MoveEmailsAsync(null);

            // Assert
            await act.Should()
                .NotThrowAsync(
                    because: "a failed move for one message must not abort the batch; issue #473 "
                        + "defect 2 keeps the log-and-proceed behaviour and changes only the number "
                        + "of log entries"
                );
            mailItem.VerifyGet(
                mail => mail.Subject,
                Times.Never(),
                "issue #473 defect 2 requires the catch to log once and return rather than "
                    + "dereferencing the same failed group a second time to look up its subject"
            );
        }

        /// <summary>
        /// Issue #473 defect 2. Covers the genuine null-group path through the boundary guard in
        /// <c>TryMoveEmailByGroupIndexAsync</c>.
        /// <para>
        /// Scenario: the cached move collection holds a single <see langword="null"/> element, so
        /// <c>TryGetItemGroupByIndex</c> resolves an in-range index to <see langword="null"/> â€”
        /// exactly what the index lookup is contracted to return for a missing group. Expected
        /// outcome: <c>MoveEmailsAsync</c> completes without throwing.
        /// </para>
        /// <para>
        /// This is the case the boundary guard exists for. Without it the null reaches
        /// <c>group.ItemController</c> inside <c>TryMoveEmailByGroupAsync</c> and is only contained
        /// by the broad catch, which is precisely the "catch instead of guard" shape the fix
        /// removes. A null element cannot be produced through <c>CacheItemGroupsForMove</c>, so it
        /// is injected directly.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_itemGroupsToMove",
                new List<QfcItemGroup> { null }
            );

            // Act
            Func<Task> act = () => controller.MoveEmailsAsync(null);

            // Assert
            await act.Should()
                .NotThrowAsync(
                    because: "issue #473 defect 2 guards the possibly-null group at the "
                        + "TryMoveEmailByGroupIndexAsync boundary rather than letting it reach the "
                        + "dereference and be contained by a broad catch"
                );
        }

        /// <summary>
        /// Invokes the private <c>TryGetItemGroupByIndex</c> and returns its result.
        /// </summary>
        private static QfcItemGroup ResolveByIndex(QfcCollectionController controller, int index)
        {
            return (QfcItemGroup)
                QfcCollectionControllerTestSupport.InvokeNonPublic(
                    controller,
                    "TryGetItemGroupByIndex",
                    index
                );
        }
    }
}
