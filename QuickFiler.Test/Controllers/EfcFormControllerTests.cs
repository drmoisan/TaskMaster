using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcFormControllerTests
    {
        /// <summary>
        /// Creates an EfcFormController via the private no-arg constructor, which allocates
        /// the object without initializing any sub-components, leaving all fields null.
        /// Used to exercise method-level guards without a live Outlook COM context.
        /// </summary>
        private static EfcFormController CreateMinimalController()
        {
            var ctor = typeof(EfcFormController).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null
            );
            ctor.Should().NotBeNull("private no-arg constructor must exist on EfcFormController");
            return (EfcFormController)ctor.Invoke(Array.Empty<object>());
        }

        // Regression test for issue #145. A minimally constructed controller now returns
        // before touching `_dataModel` when `_formViewer` has already been cleared, which is
        // the safety contract that prevents the post-await null race from surfacing as a UI
        // thread crash.
        [TestMethod]
        public async Task PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel()
        {
            // Arrange
            // Both `_dataModel` and `_formViewer` are null in a minimally constructed
            // controller. The current contract is that `PopulateFolderCombobox` should exit
            // immediately when the viewer has already been cleaned up, which also avoids
            // dereferencing `_dataModel` in this COM-free test path.
            var controller = CreateMinimalController();

            // Act
            Func<Task> act = () => controller.PopulateFolderCombobox();

            // Assert
            await act.Should()
                .NotThrowAsync(
                    "PopulateFolderCombobox should return immediately when Cleanup has already"
                        + " cleared the form viewer, instead of dereferencing downstream state"
                );
        }
    }
}
