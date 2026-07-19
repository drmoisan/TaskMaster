using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for <see cref="BreadcrumbMessageCodec"/> (#349): JSON round-trips for every
    /// inbound and outbound message type, requestId correlation on subfolderResult, and
    /// malformed-input negatives asserting <see cref="BreadcrumbMessageException"/>.
    /// </summary>
    [TestClass]
    public class BreadcrumbMessageCodecTests
    {
        private readonly BreadcrumbMessageCodec _codec = new BreadcrumbMessageCodec();

        // ---- Inbound round-trips (one per inbound type) ----

        [TestMethod]
        public void DeserializeInbound_SegmentDoubleClick_RoundTripsAllFields()
        {
            // Arrange
            string json =
                "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-3\",\"segmentIndex\":1}";

            // Act
            var message = _codec.DeserializeInbound(json);

            // Assert
            message.Type.Should().Be(BreadcrumbMessageTypes.SegmentDoubleClick);
            message.RowId.Should().Be("row-3");
            message.SegmentIndex.Should().Be(1);
            message.Key.Should().BeNull();
        }

        [TestMethod]
        public void DeserializeInbound_LeafExpandToggle_RoundTrips()
        {
            // Arrange
            string json = "{\"type\":\"leafExpandToggle\",\"rowId\":\"row-0\"}";

            // Act
            var message = _codec.DeserializeInbound(json);

            // Assert
            message.Type.Should().Be(BreadcrumbMessageTypes.LeafExpandToggle);
            message.RowId.Should().Be("row-0");
            message.SegmentIndex.Should().BeNull();
        }

        [TestMethod]
        public void DeserializeInbound_ArrowKey_RoundTripsKey()
        {
            // Arrange
            string json = "{\"type\":\"arrowKey\",\"rowId\":\"row-2\",\"key\":\"Up\"}";

            // Act
            var message = _codec.DeserializeInbound(json);

            // Assert
            message.Type.Should().Be(BreadcrumbMessageTypes.ArrowKey);
            message.RowId.Should().Be("row-2");
            message.Key.Should().Be("Up");
        }

        [TestMethod]
        public void DeserializeInbound_RowSelected_RoundTrips()
        {
            // Arrange
            string json = "{\"type\":\"rowSelected\",\"rowId\":\"row-7\"}";

            // Act
            var message = _codec.DeserializeInbound(json);

            // Assert
            message.Type.Should().Be(BreadcrumbMessageTypes.RowSelected);
            message.RowId.Should().Be("row-7");
        }

        // ---- Outbound round-trips (one per outbound type) ----

        [TestMethod]
        public void SerializeOutbound_RenderMessage_EmitsCamelCaseDiscriminatedJson()
        {
            // Arrange
            var message = new BreadcrumbRenderMessage("<html></html>", "row-1");

            // Act
            var json = JObject.Parse(_codec.SerializeOutbound(message));

            // Assert
            json["type"]!.Value<string>().Should().Be("render");
            json["html"]!.Value<string>().Should().Be("<html></html>");
            json["rowId"]!.Value<string>().Should().Be("row-1");
        }

        [TestMethod]
        public void SerializeOutbound_RenderMessage_OmitsNullRowIdForFullDocument()
        {
            // Arrange
            var message = new BreadcrumbRenderMessage("<html></html>", null);

            // Act
            var json = JObject.Parse(_codec.SerializeOutbound(message));

            // Assert: null optional field omitted per NullValueHandling.Ignore.
            json.ContainsKey("rowId").Should().BeFalse();
        }

        [TestMethod]
        public void SerializeOutbound_SubfolderResult_CarriesRequestIdCorrelationAndChildren()
        {
            // Arrange: correlated response payload with one child segment.
            var children = new[] { new BreadcrumbSegment(@"Inbox\Sub", "Sub", false) };
            var message = new BreadcrumbSubfolderResultMessage("req-42", "row-5", children);

            // Act
            var json = JObject.Parse(_codec.SerializeOutbound(message));

            // Assert: the requestId correlates the result to its originating expand request.
            json["type"]!.Value<string>().Should().Be("subfolderResult");
            json["requestId"]!.Value<string>().Should().Be("req-42");
            json["rowId"]!.Value<string>().Should().Be("row-5");
            var child = (JObject)json["children"]![0]!;
            child["fullPath"]!.Value<string>().Should().Be(@"Inbox\Sub");
            child["displayName"]!.Value<string>().Should().Be("Sub");
            child["hasSubfolders"]!.Value<bool>().Should().BeFalse();
        }

        [TestMethod]
        public void SerializeOutbound_FocusSearch_EmitsTypeOnly()
        {
            // Arrange
            var message = new BreadcrumbFocusSearchMessage();

            // Act
            var json = JObject.Parse(_codec.SerializeOutbound(message));

            // Assert
            json["type"]!.Value<string>().Should().Be("focusSearch");
        }

        [TestMethod]
        public void SerializeOutbound_WithNullMessage_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => _codec.SerializeOutbound(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        // ---- Malformed-input negatives (specific exception, no silent swallow) ----

        [TestMethod]
        public void DeserializeInbound_WithInvalidJson_ThrowsBreadcrumbMessageException()
        {
            // Act
            Action act = () => _codec.DeserializeInbound("{not valid json");

            // Assert
            act.Should()
                .Throw<BreadcrumbMessageException>()
                .WithMessage("*not valid JSON*")
                .WithInnerException<Newtonsoft.Json.JsonException>();
        }

        [TestMethod]
        public void DeserializeInbound_WithUnknownType_ThrowsBreadcrumbMessageException()
        {
            // Act
            Action act = () =>
                _codec.DeserializeInbound("{\"type\":\"teleport\",\"rowId\":\"row-1\"}");

            // Assert
            act.Should().Throw<BreadcrumbMessageException>().WithMessage("*Unknown*teleport*");
        }

        [TestMethod]
        public void DeserializeInbound_WithMissingRowId_ThrowsBreadcrumbMessageException()
        {
            // Act
            Action act = () => _codec.DeserializeInbound("{\"type\":\"rowSelected\"}");

            // Assert
            act.Should().Throw<BreadcrumbMessageException>().WithMessage("*rowId*");
        }

        [TestMethod]
        public void DeserializeInbound_WithWrongTypedRowId_ThrowsBreadcrumbMessageException()
        {
            // Act: rowId must be a string, not a number.
            Action act = () => _codec.DeserializeInbound("{\"type\":\"rowSelected\",\"rowId\":42}");

            // Assert
            act.Should().Throw<BreadcrumbMessageException>().WithMessage("*rowId*string*");
        }

        [TestMethod]
        public void DeserializeInbound_WithWrongTypedSegmentIndex_ThrowsBreadcrumbMessageException()
        {
            // Act: segmentIndex must be an integer, not a string.
            Action act = () =>
                _codec.DeserializeInbound(
                    "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-1\",\"segmentIndex\":\"first\"}"
                );

            // Assert
            act.Should().Throw<BreadcrumbMessageException>().WithMessage("*segmentIndex*integer*");
        }

        [TestMethod]
        public void DeserializeInbound_SegmentDoubleClickWithoutIndex_ThrowsBreadcrumbMessageException()
        {
            // Act: segmentDoubleClick requires segmentIndex.
            Action act = () =>
                _codec.DeserializeInbound("{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-1\"}");

            // Assert
            act.Should().Throw<BreadcrumbMessageException>().WithMessage("*segmentIndex*");
        }

        [TestMethod]
        public void DeserializeInbound_ArrowKeyWithoutKey_ThrowsBreadcrumbMessageException()
        {
            // Act: arrowKey requires a non-empty key.
            Action act = () =>
                _codec.DeserializeInbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\"}");

            // Assert
            act.Should().Throw<BreadcrumbMessageException>().WithMessage("*key*");
        }

        [TestMethod]
        public void DeserializeInbound_WithEmptyPayload_ThrowsBreadcrumbMessageException()
        {
            // Act
            Action act = () => _codec.DeserializeInbound("   ");

            // Assert
            act.Should().Throw<BreadcrumbMessageException>().WithMessage("*null or empty*");
        }
    }
}
