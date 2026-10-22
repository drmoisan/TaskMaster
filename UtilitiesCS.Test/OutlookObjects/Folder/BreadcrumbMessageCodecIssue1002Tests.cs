#nullable enable
using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue 1002 codec contract: the descendant toggle and activate inbound types carry a row
    /// identity plus a non-empty string node identity, and every malformed node identity fails
    /// fast with <see cref="BreadcrumbMessageException"/>.
    /// </summary>
    [TestClass]
    public class BreadcrumbMessageCodecIssue1002Tests
    {
        private const string NodeId = "0123456789abcdef0123456789abcdef";

        private readonly BreadcrumbMessageCodec _codec = new BreadcrumbMessageCodec();

        [TestMethod]
        public void DeserializeInbound_DescendantToggle_ReturnsNodeId()
        {
            // Arrange
            string json =
                "{\"type\":\"descendantToggle\",\"rowId\":\"row-0\",\"nodeId\":\"" + NodeId + "\"}";

            // Act
            BreadcrumbInboundMessage message = _codec.DeserializeInbound(json);

            // Assert
            message.Type.Should().Be(BreadcrumbMessageTypes.DescendantToggle);
            message.RowId.Should().Be("row-0");
            message.NodeId.Should().Be(NodeId, "the node identity is the descendant address");
            message.SegmentIndex.Should().BeNull();
            message.ChildIndex.Should().BeNull();
        }

        [TestMethod]
        public void DeserializeInbound_DescendantActivate_ReturnsNodeId()
        {
            // Arrange
            string json =
                "{\"type\":\"descendantActivate\",\"rowId\":\"row-2\",\"nodeId\":\""
                + NodeId
                + "\"}";

            // Act
            BreadcrumbInboundMessage message = _codec.DeserializeInbound(json);

            // Assert
            message.Type.Should().Be(BreadcrumbMessageTypes.DescendantActivate);
            message.RowId.Should().Be("row-2");
            message.NodeId.Should().Be(NodeId, "the node identity is the descendant address");
        }

        [DataTestMethod]
        [DataRow("descendantToggle")]
        [DataRow("descendantActivate")]
        public void DeserializeInbound_DescendantMessageWithoutNodeId_Throws(string type)
        {
            // Arrange
            string json = "{\"type\":\"" + type + "\",\"rowId\":\"row-0\"}";

            // Act
            Action act = () => _codec.DeserializeInbound(json);

            // Assert
            act.Should()
                .Throw<BreadcrumbMessageException>()
                .WithMessage("*nodeId*", "a missing node identity is reported by field name");
        }

        [DataTestMethod]
        [DataRow("descendantToggle")]
        [DataRow("descendantActivate")]
        public void DeserializeInbound_DescendantMessageWithEmptyNodeId_Throws(string type)
        {
            // Arrange
            string json = "{\"type\":\"" + type + "\",\"rowId\":\"row-0\",\"nodeId\":\"\"}";

            // Act
            Action act = () => _codec.DeserializeInbound(json);

            // Assert
            act.Should()
                .Throw<BreadcrumbMessageException>()
                .WithMessage("*nodeId*", "an empty node identity is reported by field name");
        }

        [DataTestMethod]
        [DataRow("descendantToggle")]
        [DataRow("descendantActivate")]
        public void DeserializeInbound_DescendantMessageWithNonStringNodeId_Throws(string type)
        {
            // Arrange
            string json = "{\"type\":\"" + type + "\",\"rowId\":\"row-0\",\"nodeId\":7}";

            // Act
            Action act = () => _codec.DeserializeInbound(json);

            // Assert
            act.Should()
                .Throw<BreadcrumbMessageException>()
                .WithMessage("*nodeId*", "a non-string node identity is reported by field name");
        }
    }
}
