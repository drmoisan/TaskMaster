#nullable enable
using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>Failure-first typed selector-message and wire-format contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbSelectorMessagesTests
    {
        [DataTestMethod]
        [DataRow("Collapsed", false)]
        [DataRow("Expanded", true)]
        public void ViewMessage_RoundTripsModeOpenAndStableIdentities(string modeName, bool open)
        {
            // Arrange
            Type messageType = RequireType("BreadcrumbSelectorViewMessage");
            object mode = Enum.Parse(RequireType("BreadcrumbSelectorViewMode"), modeName);
            object message = Activator.CreateInstance(
                messageType,
                mode,
                open,
                "folder-a",
                "folder-b"
            )!;

            // Act
            object parsed = Parse(Serialize(message));

            // Assert
            parsed.GetType().Should().Be(messageType);
            Property(parsed, "Mode").ToString().Should().Be(modeName);
            Property<bool>(parsed, "IsOpen").Should().Be(open);
            Property<string>(parsed, "CommittedIdentity").Should().Be("folder-a");
            Property<string>(parsed, "PendingIdentity").Should().Be("folder-b");
        }

        [TestMethod]
        public void ToggleMessage_RoundTripsWithoutSelectionPayload()
        {
            // Arrange
            object message = Activator.CreateInstance(
                RequireType("BreadcrumbSelectorToggleMessage")
            )!;

            // Act
            object parsed = Parse(Serialize(message));

            // Assert
            parsed.GetType().Name.Should().Be("BreadcrumbSelectorToggleMessage");
            Property<string>(parsed, "Type").Should().Be("selectorToggle");
        }

        /// <summary>Every supported selector key round-trips; #1007 AC8 and AC11 add Parent, Home, End and the page keys.</summary>
        [DataTestMethod]
        [DataRow("Up")]
        [DataRow("Down")]
        [DataRow("Enter")]
        [DataRow("Escape")]
        [DataRow("Parent")]
        [DataRow("Home")]
        [DataRow("End")]
        [DataRow("PageUp")]
        [DataRow("PageDown")]
        public void SelectorKeyMessage_RoundTripsOnlySupportedKeys(string keyName)
        {
            // Arrange
            Type keyType = RequireType("BreadcrumbSelectorKey");
            object message = Activator.CreateInstance(
                RequireType("BreadcrumbSelectorKeyMessage"),
                Enum.Parse(keyType, keyName)
            )!;

            // Act
            object parsed = Parse(Serialize(message));

            // Assert
            Property(parsed, "Key").ToString().Should().Be(keyName);
        }

        /// <summary>#1007 AC11: a page key carries the fully visible row count; other keys carry none.</summary>
        [TestMethod]
        public void SelectorKeyMessage_PageSize_RoundTrips()
        {
            // Arrange
            var paged = new BreadcrumbSelectorKeyMessage(BreadcrumbSelectorKey.PageDown, 7);
            var plain = new BreadcrumbSelectorKeyMessage(BreadcrumbSelectorKey.Home);

            // Act
            var parsedPaged = (BreadcrumbSelectorKeyMessage)Parse(Serialize(paged));
            var parsedPlain = (BreadcrumbSelectorKeyMessage)Parse(Serialize(plain));

            // Assert
            parsedPaged.Key.Should().Be(BreadcrumbSelectorKey.PageDown);
            parsedPaged.PageSize.Should().Be(7);
            parsedPlain.PageSize.Should().BeNull();
        }

        /// <summary>#1007 AC4: the view message carries the pending descendant node identity.</summary>
        [TestMethod]
        public void ViewMessage_PendingNodeIdentity_RoundTrips()
        {
            // Arrange
            var message = new BreadcrumbSelectorViewMessage(
                BreadcrumbSelectorViewMode.Expanded,
                true,
                "folder-a",
                "folder-b",
                "node-1"
            );

            // Act
            var parsed = (BreadcrumbSelectorViewMessage)Parse(Serialize(message));

            // Assert
            parsed.PendingIdentity.Should().Be("folder-b");
            parsed.PendingNodeIdentity.Should().Be("node-1");
        }

        [TestMethod]
        public void ActivationMessage_RoundTripsStableIdentity()
        {
            // Arrange
            object message = Activator.CreateInstance(
                RequireType("BreadcrumbSelectorActivationMessage"),
                "folder-b"
            )!;

            // Act
            object parsed = Parse(Serialize(message));

            // Assert
            Property<string>(parsed, "Identity").Should().Be("folder-b");
        }

        [TestMethod]
        public void SubfolderActivationMessage_RoundTripsUniqueRowIdentityAndSubfolderIndex()
        {
            // Arrange
            Type messageType = RequireType("BreadcrumbSelectorSubfolderActivationMessage");
            object message = Activator.CreateInstance(messageType, "suggestion:folder-b:0", 2)!;

            // Act
            string json = Serialize(message);
            object parsed = Parse(json);

            // Assert
            json.Should().Contain("\"type\":\"selectorSubfolderActivate\"");
            json.Should().Contain("\"rowIdentity\":\"suggestion:folder-b:0\"");
            json.Should().Contain("\"subfolderIndex\":2");
            parsed.GetType().Should().Be(messageType);
            Property<string>(parsed, "Type").Should().Be("selectorSubfolderActivate");
            Property<string>(parsed, "RowIdentity").Should().Be("suggestion:folder-b:0");
            Property<int>(parsed, "SubfolderIndex").Should().Be(2);
        }

        [TestMethod]
        public void SubfolderActivationConstructor_RejectsBlankIdentityAndNegativeIndex()
        {
            // Arrange
            Type messageType = RequireType("BreadcrumbSelectorSubfolderActivationMessage");
            Action blankIdentity = () => Activator.CreateInstance(messageType, " ", 0);
            Action negativeIndex = () => Activator.CreateInstance(messageType, "row-a", -1);

            // Act and assert
            blankIdentity
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentException>();
            negativeIndex
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentOutOfRangeException>();
        }

        [DataTestMethod]
        [DataRow("{\"type\":\"selectorSubfolderActivate\",\"subfolderIndex\":0}", "rowIdentity")]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\" \",\"subfolderIndex\":0}",
            "rowIdentity"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\"}",
            "subfolderIndex"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":-1}",
            "subfolderIndex"
        )]
        public void Parse_InvalidSubfolderActivationPayload_RejectsExplicitly(
            string json,
            string expected
        )
        {
            AssertParseFormatException(json, expected);
        }

        [DataTestMethod]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":1,\"subfolderIndex\":0}",
            "rowIdentity"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":1.5,\"subfolderIndex\":0}",
            "rowIdentity"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":true,\"subfolderIndex\":0}",
            "rowIdentity"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":null,\"subfolderIndex\":0}",
            "rowIdentity"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":[],\"subfolderIndex\":0}",
            "rowIdentity"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":{},\"subfolderIndex\":0}",
            "rowIdentity"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":\"0\"}",
            "subfolderIndex"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":1.5}",
            "subfolderIndex"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":true}",
            "subfolderIndex"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":null}",
            "subfolderIndex"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":[]}",
            "subfolderIndex"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":{}}",
            "subfolderIndex"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":2147483648}",
            "subfolderIndex"
        )]
        [DataRow(
            "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"row-a\",\"subfolderIndex\":999999999999999999999999999999999999999}",
            "subfolderIndex"
        )]
        public void Parse_SubfolderActivationWrongTokenKindsAndOverflow_RejectAsFormatException(
            string json,
            string expected
        )
        {
            AssertParseFormatException(json, expected);
        }

        [TestMethod]
        public void Constructors_RejectBlankStableIdentities()
        {
            // Arrange
            Action blankCommitted = () =>
                new BreadcrumbSelectorViewMessage(
                    BreadcrumbSelectorViewMode.Collapsed,
                    false,
                    " ",
                    null
                );
            Action blankPending = () =>
                new BreadcrumbSelectorViewMessage(
                    BreadcrumbSelectorViewMode.Expanded,
                    true,
                    null,
                    " "
                );
            Action blankActivation = () => new BreadcrumbSelectorActivationMessage(" ");

            // Act and assert
            blankCommitted.Should().Throw<ArgumentException>();
            blankPending.Should().Throw<ArgumentException>();
            blankActivation.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Parse_MalformedUnknownAndBlankOptionalPayloads_RejectsExplicitly()
        {
            // Arrange
            Action nullJson = () => BreadcrumbSelectorMessageSerializer.Parse(null!);
            Action malformedJson = () => BreadcrumbSelectorMessageSerializer.Parse("{");
            Action unknownType = () =>
                BreadcrumbSelectorMessageSerializer.Parse("{\"type\":\"unsupported\"}");
            Action blankCommitted = () =>
                BreadcrumbSelectorMessageSerializer.Parse(
                    "{\"type\":\"selectorView\",\"mode\":\"collapsed\",\"isOpen\":false,\"committedIdentity\":\" \"}"
                );
            Action blankPending = () =>
                BreadcrumbSelectorMessageSerializer.Parse(
                    "{\"type\":\"selectorView\",\"mode\":\"expanded\",\"isOpen\":true,\"pendingIdentity\":\" \"}"
                );

            // Act and assert
            nullJson.Should().Throw<FormatException>();
            malformedJson.Should().Throw<FormatException>();
            unknownType.Should().Throw<FormatException>();
            blankCommitted.Should().Throw<FormatException>();
            blankPending.Should().Throw<FormatException>();
        }

        [TestMethod]
        public void Serialize_NullAndUnsupportedMessage_RejectsExplicitly()
        {
            // Arrange
            Action nullMessage = () => BreadcrumbSelectorMessageSerializer.Serialize(null!);
            Action unsupportedMessage = () =>
                BreadcrumbSelectorMessageSerializer.Serialize(new UnsupportedSelectorMessage());

            // Act and assert
            nullMessage.Should().Throw<ArgumentNullException>();
            unsupportedMessage.Should().Throw<FormatException>();
        }

        [DataTestMethod]
        [DataRow("{\"type\":\"selectorActivate\"}", "identity")]
        [DataRow("{\"type\":\"selectorActivate\",\"identity\":\"\"}", "identity")]
        [DataRow("{\"type\":\"selectorKey\",\"key\":\"left\"}", "selector key")]
        [DataRow("{\"type\":\"selectorView\",\"mode\":\"grid\",\"isOpen\":true}", "view mode")]
        public void Parse_InvalidIdentityKeyOrMode_RejectsExplicitly(string json, string expected)
        {
            // Arrange
            MethodInfo parse = SerializerType().GetMethod("Parse")!;

            // Act
            Action act = () => parse.Invoke(null, new object[] { json });

            // Assert
            act.Should().Throw<TargetInvocationException>().WithInnerException<FormatException>();
            try
            {
                parse.Invoke(null, new object[] { json });
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException!.Message.Should().ContainEquivalentOf(expected);
            }
        }

        /// <summary>#1007 AC9 (S5): the segment double-click carries the row identity and segment index.</summary>
        [TestMethod]
        public void SegmentDoubleClickMessage_RoundTripsRowIdentityAndSegmentIndex()
        {
            // Arrange
            var message = new BreadcrumbSelectorSegmentDoubleClickMessage("row-a", 1);

            // Act
            var parsed = (BreadcrumbSelectorSegmentDoubleClickMessage)Parse(Serialize(message));

            // Assert
            parsed.Type.Should().Be("selectorSegmentDoubleClick");
            parsed.RowIdentity.Should().Be("row-a");
            parsed.SegmentIndex.Should().Be(1);
        }

        /// <summary>#1007 AC9 (D-Q27): the open-list highlight carries the row identity.</summary>
        [TestMethod]
        public void HighlightMessage_RoundTripsIdentity()
        {
            // Arrange
            var message = new BreadcrumbSelectorHighlightMessage("row-b");

            // Act
            var parsed = (BreadcrumbSelectorHighlightMessage)Parse(Serialize(message));

            // Assert
            parsed.Type.Should().Be("selectorHighlight");
            parsed.Identity.Should().Be("row-b");
        }

        /// <summary>#1007 S5: invalid double-click and highlight payloads are rejected explicitly.</summary>
        [TestMethod]
        public void Parse_SegmentDoubleClickWithNegativeIndex_RejectsExplicitly()
        {
            // Arrange
            const string Negative =
                "{\"type\":\"selectorSegmentDoubleClick\",\"rowIdentity\":\"row-a\",\"segmentIndex\":-1}";
            const string Blank =
                "{\"type\":\"selectorSegmentDoubleClick\",\"rowIdentity\":\" \",\"segmentIndex\":0}";
            Action blankHighlight = () => new BreadcrumbSelectorHighlightMessage(" ");
            Action blankRow = () => new BreadcrumbSelectorSegmentDoubleClickMessage(" ", 0);
            Action negativeIndex = () =>
                new BreadcrumbSelectorSegmentDoubleClickMessage("row-a", -1);

            // Act / Assert
            AssertParseFormatException(Negative, "segmentIndex");
            AssertParseFormatException(Blank, "rowIdentity");
            AssertParseFormatException(
                "{\"type\":\"selectorHighlight\",\"identity\":\"\"}",
                "identity"
            );
            blankHighlight.Should().Throw<ArgumentException>();
            blankRow.Should().Throw<ArgumentException>();
            negativeIndex.Should().Throw<ArgumentOutOfRangeException>();
        }

        private static object Parse(string json) =>
            SerializerType().GetMethod("Parse")!.Invoke(null, new object[] { json })!;

        private static void AssertParseFormatException(string json, string expected)
        {
            MethodInfo parse = SerializerType().GetMethod("Parse")!;
            Action act = () => parse.Invoke(null, new object[] { json });
            act.Should().Throw<TargetInvocationException>().WithInnerException<FormatException>();
            try
            {
                parse.Invoke(null, new object[] { json });
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException!.Message.Should().ContainEquivalentOf(expected);
            }
        }

        private static string Serialize(object message) =>
            (string)SerializerType().GetMethod("Serialize")!.Invoke(null, new[] { message })!;

        private static Type SerializerType() => RequireType("BreadcrumbSelectorMessageSerializer");

        private static Type RequireType(string shortName)
        {
            Type? type = typeof(BreadcrumbStateModel).Assembly.GetType(
                $"UtilitiesCS.OutlookObjects.Folder.{shortName}",
                false
            );
            type.Should().NotBeNull($"issue #400 requires {shortName}");
            return type!;
        }

        private static object Property(object target, string property) =>
            target.GetType().GetProperty(property)!.GetValue(target)!;

        private static T Property<T>(object target, string property) =>
            (T)Property(target, property);

        private sealed class UnsupportedSelectorMessage : BreadcrumbSelectorMessage
        {
            public override string Type => "unsupported";
        }
    }
}
