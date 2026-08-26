using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;

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
    public class QfcCollectionControllerDefects468Tests
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
        /// Issue #474 defect 1. Structural regression test asserting that
        /// <c>QfcCollectionController</c> holds its parent by the wider
        /// <see cref="QuickFiler.Controllers.IQfcFormController"/> contract rather than by
        /// <c>QuickFiler.Interfaces.IFilerFormController</c>.
        /// <para>
        /// Scenario: read the declared type of the private <c>_parent</c> field and the declared
        /// type of the fifth parameter of the controller's only public constructor. Expected
        /// outcome: both are <c>QuickFiler.Controllers.IQfcFormController</c>.
        /// </para>
        /// <para>
        /// This is a structural assertion rather than a behavioural one because the defect's
        /// observable symptom — the <c>(QfcFormController)_parent</c> downcast throwing
        /// <c>InvalidCastException</c> — sits behind <c>await UiThread.Dispatcher.InvokeAsync(...)</c>,
        /// and <c>UiThread.Init()</c> shows a window, which the repository unit-test policy
        /// prohibits. The declared types are the proof that the runtime cast was replaced by a
        /// compile-time constraint. The same species of structural guard is established repository
        /// practice in <c>QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs</c>.
        /// </para>
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
        /// Issue #286. Regression test proving the reentrancy counter is restored when
        /// <c>RemoveSpecificControlGroupAsync</c> throws at the very first statement after the
        /// <c>Interlocked.Increment</c>.
        /// <para>
        /// Scenario: an uninitialized controller leaves <c>_itemGroups</c> <c>null</c>, so
        /// <c>UnregisterNavigation()</c> â€” the statement immediately following the increment â€”
        /// dereferences <c>null</c> and raises <see cref="NullReferenceException"/>. Expected
        /// outcome: the exception propagates, and the private static counter is back at its
        /// pre-call value because the decrement runs on the exceptional path too.
        /// </para>
        /// <para>
        /// Before the fix the decrement is the method's last statement and is unreachable after a
        /// throw, so the counter is left one higher than its pre-call value. The leak is permanent
        /// for the life of the process and eventually trips the
        /// <c>"Counter is greater than 1. Race Condition Exists"</c> error branch on a subsequent
        /// legitimate call.
        /// </para>
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
                    because: "UnregisterNavigation() is the first statement after the increment and "
                        + "it dereferences the null _itemGroups field"
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
        /// <see cref="RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter"/>,
        /// proving the restored decrement covers the <em>whole</em> body rather than only the first
        /// statement.
        /// <para>
        /// Scenario: the controller is arranged so <c>UnregisterNavigation()</c> completes
        /// successfully â€” a real empty <c>KbdActions</c> collection is supplied to the mocked
        /// keyboard handler and a single item group is injected â€” and the throw is instead raised
        /// several statements later, by the mocked <c>IsActiveUI</c> getter. Expected outcome: the
        /// exception propagates and the private static counter is back at its pre-call value.
        /// </para>
        /// <para>
        /// A fix that guarded only the first statement would pass the companion test and fail this
        /// one, so the pair together pin the <c>finally</c> to the full span between the increment
        /// and the decrement.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter()
        {
            // Arrange
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();

            // A real (empty) KbdActions instance rather than a mock: UnregisterNavigation calls
            // Remove(...) on it directly, and it must succeed so the throw lands later in the body.
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
    }
}
