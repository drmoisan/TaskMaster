using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #677: the item-controller hop of the deactivate selector-cancel fan-out. The form
    /// controller calls <c>IQfcItemController.CancelBreadcrumbSelector()</c> on every item group;
    /// this class pins the controller's forwarding contract to the narrowed viewer seam.
    /// <para>
    /// Modeled on the construction / <c>SetViewer</c> private-field-injection pattern in
    /// <c>QfcItemController.PropertiesTests.cs</c>: a minimal derived controller reaches the
    /// parameterless base constructor, and the viewer field is assigned by reflection so no live
    /// WinForms control tree is required.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcItemControllerCancelBreadcrumbSelectorTests
    {
        private sealed class CancelController : QfcItemController
        {
            internal CancelController()
                : base() { }
        }

        private static void SetViewer(QfcItemController controller, IItemViewer viewer) =>
            typeof(QfcItemController)
                .GetField("_itemViewer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, viewer);

        /// <summary>The cancel intent forwards straight through to the viewer seam exactly once.</summary>
        [TestMethod]
        public void CancelBreadcrumbSelector_ForwardsToViewer()
        {
            // Arrange
            var controller = new CancelController();
            var viewer = new Mock<IItemViewer>();
            SetViewer(controller, viewer.Object);

            // Act
            controller.CancelBreadcrumbSelector();

            // Assert
            viewer.Verify(x => x.CancelBreadcrumbSelector(), Times.Once());
        }

        /// <summary>
        /// A controller whose viewer has already been released (cleanup, or an item that never
        /// finished loading) must be a safe no-op, because the deactivate fan-out reaches every
        /// item group regardless of its lifecycle state.
        /// </summary>
        [TestMethod]
        public void CancelBreadcrumbSelector_NullViewer_DoesNotThrow()
        {
            // Arrange
            var controller = new CancelController();
            SetViewer(controller, null);

            // Act
            Action act = () => controller.CancelBreadcrumbSelector();

            // Assert
            act.Should().NotThrow();
        }
    }
}
