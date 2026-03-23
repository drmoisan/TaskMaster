using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcHomeControllerTests
    {
        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<System.Action> _mockParentCleanup;

        [TestInitialize]
        public void Setup()
        {
            _mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            _mockParentCleanup = new Mock<System.Action>();
        }

        /// <summary>
        /// Creates an EfcHomeController via the private (globals, parentCleanup) constructor,
        /// which does not allocate sub-components such as the data model or stop-watch.
        /// </summary>
        private EfcHomeController CreateMinimalController()
        {
            var ctor = typeof(EfcHomeController).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(IApplicationGlobals), typeof(System.Action) },
                null
            );
            ctor.Should().NotBeNull("private (globals, parentCleanup) constructor must exist");
            return (EfcHomeController)
                ctor.Invoke(new object[] { _mockGlobals.Object, _mockParentCleanup.Object });
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.Should().NotBeNull($"field '{fieldName}' must exist on EfcHomeController");
            field.SetValue(target, value);
        }

        // Regression test for:
        // System.NullReferenceException at EfcHomeController.ExecuteMovesAsync line 346
        // Root cause: re-entrant invocation could call Cleanup() while MoveToFolderAsync was
        // awaited, nulling _globals. The second continuation then dereferenced null _globals.
        [TestMethod]
        public async Task ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields()
        {
            // Arrange
            var controller = CreateMinimalController();

            // Simulate a concurrent invocation already in progress.
            SetField(controller, "_isExecuting", true);

            // _formController is null from the private constructor.
            // If the guard were absent, the very first line of ExecuteMovesAsync would throw
            // NullReferenceException on _formController.SelectedFolder.

            // Act & Assert: must complete without exception.
            Func<Task> act = () => controller.ExecuteMovesAsync();
            await act.Should()
                .NotThrowAsync("a re-entrant call must be dropped via the _isExecuting guard");
        }

        // Regression test for the inverted guard in QuickFileMetrics_WRITE.
        // The original condition was `moved.Count == 0`, which entered the metrics-writing
        // block only when the list was empty and would immediately throw DivideByZeroException
        // on `Duration /= moved.Count`. The fix changes the condition to `moved.Count > 0`.
        [TestMethod]
        public void QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow()
        {
            // Arrange
            var controller = CreateMinimalController();
            // _stopWatch is null in a controller created via the private constructor.
            // If the body were entered (old bug: Count == 0), accessing _stopWatch.Elapsed
            // would throw NullReferenceException before even reaching DivideByZeroException.
            var emptyMoved = new List<MailItemHelper>();

            // Act & Assert
            Action act = () =>
                controller.QuickFileMetrics_WRITE("session.csv", @"Inbox\Projects", emptyMoved);
            act.Should()
                .NotThrow(
                    "an empty moved list must cause the metrics body to be skipped (Count > 0 guard)"
                );
        }

        [TestMethod]
        public void QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow()
        {
            // Arrange
            var controller = CreateMinimalController();

            // Act & Assert
            Action act = () =>
                controller.QuickFileMetrics_WRITE("session.csv", @"Inbox\Projects", null);
            act.Should().NotThrow("a null moved list must be handled by the null guard");
        }
    }
}
