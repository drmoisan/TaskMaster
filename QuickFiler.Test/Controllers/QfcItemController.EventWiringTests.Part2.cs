using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Continuation of <see cref="QfcItemController_EventWiringTests"/> (issue #486 D3). The parent
    /// file sits one line under the 500-line ceiling, so these two tests live here, following the
    /// <c>QfcItemController.InitializationTests.Part2.cs</c> precedent. The
    /// <c>[TestClass]</c> attribute stays on the parent declaration only; repeating it on a second
    /// partial declaration of the same class is a compile error.
    /// </summary>
    public partial class QfcItemController_EventWiringTests
    {
        /// <summary>
        /// #486 D3: the picture-saving menu item raises <c>PicturesChanged</c>, but
        /// <c>WireIntentEvents()</c> subscribes to every sibling intent event except that one, so
        /// the controller never observes a change to the option.
        /// </summary>
        [TestMethod]
        public void WireIntentEvents_SubscribesToPicturesChanged()
        {
            // Arrange
            var viewer = new Mock<IItemViewer>();
            var kbd = new Mock<IQfcKeyboardHandler>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", kbd.Object);

            // Act
            controller.WireIntentEvents();

            // Assert
            viewer.VerifyAdd(v => v.PicturesChanged += It.IsAny<EventHandler>(), Times.Once());
        }

        /// <summary>
        /// #486 D3: once subscribed, raising <c>PicturesChanged</c> must refresh the controller's
        /// cached save-pictures option from the viewer, exactly as the conversation, email-copy and
        /// attachment siblings already do.
        /// </summary>
        [TestMethod]
        public void PicturesChanged_WhenRaised_RefreshesOptionsPictures()
        {
            // Arrange
            var viewer = new Mock<IItemViewer>();
            var kbd = new Mock<IQfcKeyboardHandler>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", kbd.Object);
            QfcItemControllerTestSupport.SetField(controller, "_optionsPictures", false);
            viewer.SetupGet(v => v.PicturesChecked).Returns(true);
            controller.WireIntentEvents();

            // Act
            viewer.Raise(v => v.PicturesChanged += null, viewer.Object, EventArgs.Empty);

            // Assert
            QfcItemControllerTestSupport
                .GetField(controller, "_optionsPictures")
                .Should()
                .Be(
                    true,
                    "the handler must read the toggled value back from the viewer when the event fires"
                );
        }

        /// <summary>
        /// RC-1 (remediation cycle 1): every event WireIntentEvents() subscribes must be detached
        /// by UnwireIntentEvents(); after Cleanup() a controller holds zero live subscriptions on
        /// its viewer. The 17th subscription (PicturesChanged, the #486 D3 fix) had no matching
        /// detachment, leaking one live subscription per torn-down controller.
        /// </summary>
        [TestMethod]
        public void UnwireIntentEvents_DetachesPicturesChanged()
        {
            // Arrange
            var viewer = new Mock<IItemViewer>();
            var kbd = new Mock<IQfcKeyboardHandler>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", kbd.Object);
            controller.WireIntentEvents();

            // Act
            controller.UnwireIntentEvents();

            // Assert
            viewer.VerifyRemove(v => v.PicturesChanged -= It.IsAny<EventHandler>(), Times.Once());
        }
    }
}
