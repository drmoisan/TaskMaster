using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Helper_Classes;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #489 D2. <c>HtmlDarkConverter</c> writes to the WebView2 surface through
    /// <c>_itemViewer.NavigateToString</c> with no <c>InvokeRequired</c> check, so a call arriving on
    /// a background thread touches the control from the wrong thread. These tests pin the guarded
    /// routing that mirrors the <c>NavigateToString</c> pair already present at
    /// <c>QfcItemController.EventWiring.cs:140-147</c>.
    ///
    /// <para>
    /// <c>IItemViewer</c> re-declares <c>InvokeRequired</c> and <c>Invoke(Delegate)</c> at
    /// <c>IItemViewer.cs:135-136</c> specifically so guarded routing stays mockable, with the
    /// rationale recorded at the surrounding <c>#pragma warning disable CS0108</c>. A
    /// <c>Mock&lt;IItemViewer&gt;</c> with a stubbed <c>InvokeRequired</c> is therefore sufficient:
    /// no STA host, no message pump, and no form is constructed anywhere in this file.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcItemController_ThemeMarshallingTests
    {
        /// <summary>
        /// Builds a controller whose viewer reports the requested <c>InvokeRequired</c> state. The
        /// viewer mock deliberately leaves <c>Invoke</c> unconfigured so it records the call without
        /// running the marshaled delegate; that is what lets a test distinguish "handed to the UI
        /// thread" from "executed inline on the calling thread".
        /// </summary>
        private static (HarnessController controller, Mock<IItemViewer> viewer) BuildController(
            bool invokeRequired
        )
        {
            Mock<IUiDispatcher> dispatcher = QfcItemControllerTestSupport.BuildSyncDispatcher();
            var viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(invokeRequired);

            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_uiDispatcher", dispatcher.Object);
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_isWebViewerInitialized", true);

            var mailItem = new Mock<Microsoft.Office.Interop.Outlook.MailItem>();
            var globals = new Mock<IApplicationGlobals>();

            // The parameterless MailItemHelper constructor runs InitializeSafeDefaults(), which seeds
            // every Lazy field with an empty value. The MailItem-backed overload instead wires Html to
            // a Lazy that calls GetHtml() and dereferences MailItem.HTMLBody, which a mocked COM item
            // returns as null and Regex.Replace then rejects with ArgumentNullException. Using the safe
            // constructor keeps ToggleDark a pure string transform over string.Empty, so the assertions
            // below observe the marshalling guard rather than incidental Outlook I/O.
            controller.ItemHelper = new MailItemHelper();

            // Count is the first member HtmlDarkConverter reads after NavigateToString. Left at its
            // (-1, -1) "not yet loaded" sentinel the getter would call LoadCount() and reach the
            // DataFrame, so it is seeded with a loaded, empty value. That keeps the expanded-branch
            // unentered and guarantees a failing assertion below is the guard, never an incidental
            // NullReferenceException.
            var resolver = new ConversationResolver(globals.Object, mailItem.Object);
            resolver.Count = new Pair<int>(0, 0);
            QfcItemControllerTestSupport.SetField(controller, "_conversationResolver", resolver);

            return (controller, viewer);
        }

        [TestMethod]
        public void HtmlDarkConverter_WhenInvokeRequired_MarshalsThroughInvoke()
        {
            // Arrange
            var (controller, viewer) = BuildController(invokeRequired: true);

            // Act
            controller.HtmlDarkConverter(Enums.ToggleState.On);

            // Assert
            viewer.Verify(
                v => v.Invoke(It.IsAny<Delegate>()),
                Times.Once(),
                "a theme change raised off the UI thread must be marshaled onto it exactly once"
            );
        }

        [TestMethod]
        public void HtmlDarkConverter_WhenInvokeRequired_DoesNotNavigateWithoutMarshalling()
        {
            // Arrange
            var (controller, viewer) = BuildController(invokeRequired: true);

            // Act
            controller.HtmlDarkConverter(Enums.ToggleState.On);

            // Assert
            // Invoke is recorded but never executes its delegate, so a NavigateToString observed here
            // could only have been reached by bypassing the guard and touching the control directly.
            viewer.Verify(
                v => v.NavigateToString(It.IsAny<string>()),
                Times.Never(),
                "the WebView2 surface must not be written from the calling thread when a marshal is required"
            );
        }

        [TestMethod]
        public void HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly()
        {
            // Arrange
            var (controller, viewer) = BuildController(invokeRequired: false);

            // Act
            controller.HtmlDarkConverter(Enums.ToggleState.On);

            // Assert
            viewer.Verify(
                v => v.NavigateToString(It.IsAny<string>()),
                Times.Once(),
                "already on the UI thread the converter must write through without a marshal"
            );
            viewer.Verify(
                v => v.Invoke(It.IsAny<Delegate>()),
                Times.Never(),
                "marshaling when it is not required would add a needless message-pump round trip"
            );
        }
    }
}
