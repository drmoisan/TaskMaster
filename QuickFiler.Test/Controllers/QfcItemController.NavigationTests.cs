using System;
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
    /// Navigation-cluster tests (research §5.2). Covers the pure KbdExecuteAsync keyboard-routing
    /// for both overloads: keyboard deactivation is conditional on the deactivateKbd flag, and the
    /// supplied action is always awaited.
    /// </summary>
    [TestClass]
    public class QfcItemController_NavigationTests
    {
        private sealed class NavController : QfcItemController
        {
            internal NavController(IFilerHomeController homeController)
                : base()
            {
                typeof(QfcItemController)
                    .GetField("_homeController", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, homeController);
            }
        }

        private static (NavController controller, Mock<IQfcKeyboardHandler> kbd) BuildController()
        {
            var mockKbd = new Mock<IQfcKeyboardHandler>();
            var mockHome = new Mock<IFilerHomeController>();
            mockHome.SetupGet(h => h.KeyboardHandler).Returns(mockKbd.Object);
            return (new NavController(mockHome.Object), mockKbd);
        }

        [TestMethod]
        public async Task KbdExecuteAsync_WhenDeactivateKbdTrue_TogglesKeyboardAndRunsAction()
        {
            // Arrange
            var (controller, mockKbd) = BuildController();
            var ran = false;

            // Act
            await controller.KbdExecuteAsync(
                () =>
                {
                    ran = true;
                    return Task.CompletedTask;
                },
                deactivateKbd: true
            );

            // Assert
            ran.Should().BeTrue();
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Once());
        }

        [TestMethod]
        public async Task KbdExecuteAsync_WhenDeactivateKbdFalse_RunsActionWithoutToggling()
        {
            // Arrange
            var (controller, mockKbd) = BuildController();
            var ran = false;

            // Act
            await controller.KbdExecuteAsync(
                () =>
                {
                    ran = true;
                    return Task.CompletedTask;
                },
                deactivateKbd: false
            );

            // Assert
            ran.Should().BeTrue();
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Never());
        }

        [TestMethod]
        public async Task KbdExecuteAsyncGeneric_WhenDeactivateKbdTrue_TogglesAndPassesArgument()
        {
            // Arrange
            var (controller, mockKbd) = BuildController();
            string captured = null;

            // Act
            await controller.KbdExecuteAsync(
                (string arg) =>
                {
                    captured = arg;
                    return Task.CompletedTask;
                },
                "payload",
                deactivateKbd: true
            );

            // Assert
            captured.Should().Be("payload");
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Once());
        }

        [TestMethod]
        public async Task KbdExecuteAsyncGeneric_WhenDeactivateKbdFalse_DoesNotToggle()
        {
            // Arrange
            var (controller, mockKbd) = BuildController();
            string captured = null;

            // Act
            await controller.KbdExecuteAsync(
                (string arg) =>
                {
                    captured = arg;
                    return Task.CompletedTask;
                },
                "payload",
                deactivateKbd: false
            );

            // Assert
            captured.Should().Be("payload");
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Never());
        }
    }
}
