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

        [DataTestMethod]
        [DataRow("Up")]
        [DataRow("Down")]
        [DataRow("Enter")]
        [DataRow("Escape")]
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

        private static object Parse(string json) =>
            SerializerType().GetMethod("Parse")!.Invoke(null, new object[] { json })!;

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
