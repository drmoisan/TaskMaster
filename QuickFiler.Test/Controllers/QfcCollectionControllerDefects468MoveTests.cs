using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
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
