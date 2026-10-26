using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #1007 R8, N-1: the coordinator's handling of a router <c>unhandledArrow</c> output.
    /// Split from <c>BreadcrumbBridgeCoordinatorIssue1007Tests.cs</c> to keep that file under the
    /// 500-line limit; this partial reuses its private <c>Harness</c>.
    /// </summary>
    public sealed partial class BreadcrumbBridgeCoordinatorIssue1007Tests
    {
        /// <summary>
        /// Issue #1007 R8, N-1 (D3): with the list open and the pending row expanded, an inbound
        /// <c>unhandledArrow</c> Left is classified Route. The router echoes it as its output, so
        /// the coordinator raises <c>UnhandledArrow</c> once and one synthetic key, and changes
        /// neither the open state nor the committed folder.
        /// </summary>
        [TestMethod]
        public void OpenUnhandledArrowLeft_OnExpandedPendingRow_RaisesUnhandledArrowFromRouterOutput()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true, expandAlpha: true);

            // Act
            h.Receive("{\"type\":\"unhandledArrow\",\"direction\":\"left\"}");

            // Assert
            h.Events.Should().Equal("unhandled:Left");
            h.KeyDowns.Should().Be(1, "the inbound unhandledArrow raises one synthetic key");
            h.Coordinator.IsSelectorOpen.Should().BeTrue();
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha);
        }
    }
}
