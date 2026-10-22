#nullable enable
using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue 1002 message constructors: every required argument is guarded and a null argument
    /// fails fast with <see cref="ArgumentNullException"/> naming the offending parameter.
    /// </summary>
    [TestClass]
    public class BreadcrumbMessagesIssue1002Tests
    {
        private static readonly IReadOnlyList<BreadcrumbSegment> NoChildren =
            Array.Empty<BreadcrumbSegment>();

        [TestMethod]
        public void BreadcrumbInboundMessage_NullType_ThrowsWithParamName()
        {
            // Arrange
            Action act = () => _ = new BreadcrumbInboundMessage(null!, "row-0", null, null, null);

            // Act / Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("type");
        }

        [TestMethod]
        public void BreadcrumbInboundMessage_NullRowId_ThrowsWithParamName()
        {
            // Arrange
            Action act = () => _ = new BreadcrumbInboundMessage("select", null!, null, null, null);

            // Act / Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("rowId");
        }

        [TestMethod]
        public void BreadcrumbOutboundMessage_NullType_ThrowsWithParamName()
        {
            // Arrange
            Action act = () => _ = new NullTypeOutboundMessage();

            // Act / Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("type");
        }

        [TestMethod]
        public void BreadcrumbRenderMessage_NullHtml_ThrowsWithParamName()
        {
            // Arrange
            Action act = () => _ = new BreadcrumbRenderMessage(null!, null);

            // Act / Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("html");
        }

        [TestMethod]
        public void BreadcrumbSubfolderResultMessage_NullRequestId_Throws()
        {
            // Arrange
            Action act = () => _ = new BreadcrumbSubfolderResultMessage(null!, "row-0", NoChildren);

            // Act / Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("requestId");
        }

        [TestMethod]
        public void BreadcrumbSubfolderResultMessage_NullRowId_Throws()
        {
            // Arrange
            Action act = () => _ = new BreadcrumbSubfolderResultMessage("req-0", null!, NoChildren);

            // Act / Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("rowId");
        }

        [TestMethod]
        public void BreadcrumbSubfolderResultMessage_NullChildren_Throws()
        {
            // Arrange
            Action act = () => _ = new BreadcrumbSubfolderResultMessage("req-0", "row-0", null!);

            // Act / Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("children");
        }

        // The base constructor is protected and every production subclass passes a constant, so a
        // test subclass is the only way to forward a null discriminator.
        private sealed class NullTypeOutboundMessage : BreadcrumbOutboundMessage
        {
            public NullTypeOutboundMessage()
                : base(null!) { }
        }
    }
}
