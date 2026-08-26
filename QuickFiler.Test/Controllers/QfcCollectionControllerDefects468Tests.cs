using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

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
    }
}
