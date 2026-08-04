using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for the breadcrumb bridge protocol (#351 P3-T6): round-trip serialization of
    /// every message family, explicit parse failures (malformed JSON, unknown type, missing
    /// fields), and edge payloads (empty subfolder response, maximal-length paths, non-ASCII
    /// names). Deterministic; no Outlook, WebView2, or I/O.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbBridgeMessagesTests
    {
        private static string RoundTrip(
            BreadcrumbBridgeMessage message,
            out BreadcrumbBridgeMessage parsed
        )
        {
            var json = BreadcrumbBridgeSerializer.Serialize(message);
            parsed = BreadcrumbBridgeSerializer.Parse(json);
            return json;
        }

        // --- Round trips for every message family ---

        [TestMethod]
        public void RoundTrip_SegmentDoubleClick_PreservesIndexes()
        {
            RoundTrip(new SegmentDoubleClickMessage(2, 1), out var parsed);
            var m = parsed.Should().BeOfType<SegmentDoubleClickMessage>().Subject;
            m.RowIndex.Should().Be(2);
            m.SegmentIndex.Should().Be(1);
        }

        [TestMethod]
        public void RoundTrip_AffordanceToggle_PreservesRowIndex()
        {
            RoundTrip(new AffordanceToggleMessage(4), out var parsed);
            parsed.Should().BeOfType<AffordanceToggleMessage>().Subject.RowIndex.Should().Be(4);
        }

        [TestMethod]
        public void RoundTrip_ArrowKey_PreservesBothDirections()
        {
            RoundTrip(new ArrowKeyMessage(BreadcrumbArrowDirection.Left), out var left);
            RoundTrip(new ArrowKeyMessage(BreadcrumbArrowDirection.Right), out var right);
            left.Should()
                .BeOfType<ArrowKeyMessage>()
                .Subject.Direction.Should()
                .Be(BreadcrumbArrowDirection.Left);
            right
                .Should()
                .BeOfType<ArrowKeyMessage>()
                .Subject.Direction.Should()
                .Be(BreadcrumbArrowDirection.Right);
        }

        [TestMethod]
        public void RoundTrip_SelectionChange_PreservesSubfolderIndexAndMappedFolder()
        {
            RoundTrip(
                new SelectionChangeMessage(1, 3, "\\Inbox\\Projects\\Apollo"),
                out var parsed
            );
            var m = parsed.Should().BeOfType<SelectionChangeMessage>().Subject;
            m.RowIndex.Should().Be(1);
            m.SubfolderIndex.Should().Be(3);
            m.SelectedFolder.Should().Be("\\Inbox\\Projects\\Apollo");
        }

        [TestMethod]
        public void RoundTrip_SelectionChange_WithoutFolderOrSubfolder_UsesExplicitDefaults()
        {
            RoundTrip(new SelectionChangeMessage(0, -1, null), out var parsed);
            var m = parsed.Should().BeOfType<SelectionChangeMessage>().Subject;
            m.SubfolderIndex.Should().Be(-1);
            m.SelectedFolder.Should().BeNull();
        }

        [TestMethod]
        public void RoundTrip_SubfolderRequest_PreservesRowIndex()
        {
            RoundTrip(new SubfolderRequestMessage(7), out var parsed);
            parsed.Should().BeOfType<SubfolderRequestMessage>().Subject.RowIndex.Should().Be(7);
        }

        [TestMethod]
        public void RoundTrip_SubfolderResponse_PreservesSubfolderList()
        {
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(SampleChain(), 0.4);
            model.Rows[0].TryExpandLeaf();
            model
                .Rows[0]
                .SetSubfolders(
                    new[]
                    {
                        Segment("s1", "\\Inbox\\Léaf\\Ärchiv", "Ärchiv", true),
                        Segment("s2", "\\Inbox\\Léaf\\日本語", "日本語", false),
                    }
                );
            var rendered = BreadcrumbRenderProjection.Project(model)[0].Subfolders;

            RoundTrip(new SubfolderResponseMessage(0, rendered), out var parsed);

            var m = parsed.Should().BeOfType<SubfolderResponseMessage>().Subject;
            m.Subfolders.Select(s => s.DisplayName).Should().Equal("Ärchiv", "日本語");
            m.Subfolders.Select(s => s.HasChildren).Should().Equal(true, false);
        }

        [TestMethod]
        public void RoundTrip_ThemeChange_PreservesTheme()
        {
            RoundTrip(new ThemeChangeMessage("dark"), out var parsed);
            parsed.Should().BeOfType<ThemeChangeMessage>().Subject.Theme.Should().Be("dark");
        }

        [TestMethod]
        public void RoundTrip_Render_PreservesRowsCellsAndPercentText()
        {
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(SampleChain(), 0.73);
            model.AddPlainRow("Trash to Delete");
            model.SelectRow(1);
            var rows = BreadcrumbRenderProjection.Project(model);

            var json = RoundTrip(new RenderMessage(rows), out var parsed);

            json.Should().Contain("\"type\":\"render\"");
            var m = parsed.Should().BeOfType<RenderMessage>().Subject;
            m.Rows.Should().HaveCount(2);
            m.Rows[0].PercentText.Should().Be("73%");
            m.Rows[0].Cells.Select(c => c.Kind).Should().Equal(rows[0].Cells.Select(c => c.Kind));
            m.Rows[0].Cells.Select(c => c.Text).Should().Equal(rows[0].Cells.Select(c => c.Text));
            m.Rows[1].Selected.Should().BeTrue();
            m.Rows[1].IsSuggestion.Should().BeFalse();
        }

        [TestMethod]
        public void RoundTrip_Render_PreservesSelectedChildStateAndLegacyDefaults()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(SampleChain(), 0.73);
            var rows = BreadcrumbRenderProjection.Project(model);
            const string selectedFolder = "\\Inbox\\Leaf\\Alpha";
            Type renderType = typeof(RenderMessage);
            ConstructorInfo constructor = renderType.GetConstructor(
                new[] { typeof(IReadOnlyList<BreadcrumbRowRender>), typeof(int), typeof(string) }
            );
            PropertyInfo selectedSubfolderIndex = renderType.GetProperty("SelectedSubfolderIndex");
            PropertyInfo selectedFolderProperty = renderType.GetProperty("SelectedFolder");

            // Act
            constructor
                .Should()
                .NotBeNull("render messages need a cacheable selected-child shape");
            selectedSubfolderIndex.Should().NotBeNull();
            selectedFolderProperty.Should().NotBeNull();
            var selectedMessage = (RenderMessage)
                constructor.Invoke(new object[] { rows, 0, selectedFolder });
            string selectedJson = RoundTrip(selectedMessage, out var selectedParsed);
            var legacyRoot = JObject.Parse(
                BreadcrumbBridgeSerializer.Serialize(new RenderMessage(rows))
            );
            legacyRoot.Remove("selectedSubfolderIndex");
            legacyRoot.Remove("selectedFolder");
            var legacyParsed = BreadcrumbBridgeSerializer
                .Parse(legacyRoot.ToString())
                .Should()
                .BeOfType<RenderMessage>()
                .Subject;

            // Assert
            selectedJson.Should().Contain("\"selectedSubfolderIndex\":0");
            selectedJson.Should().Contain("\"selectedFolder\":\"\\\\Inbox\\\\Leaf\\\\Alpha\"");
            selectedSubfolderIndex.GetValue(selectedParsed).Should().Be(0);
            selectedFolderProperty.GetValue(selectedParsed).Should().Be(selectedFolder);
            selectedSubfolderIndex.GetValue(legacyParsed).Should().Be(-1);
            selectedFolderProperty.GetValue(legacyParsed).Should().BeNull();
        }

        [TestMethod]
        public void RoundTrip_UnhandledArrow_PreservesDirection()
        {
            RoundTrip(new UnhandledArrowMessage(BreadcrumbArrowDirection.Right), out var parsed);
            parsed
                .Should()
                .BeOfType<UnhandledArrowMessage>()
                .Subject.Direction.Should()
                .Be(BreadcrumbArrowDirection.Right);
        }

        [TestMethod]
        public void RoundTrip_Error_PreservesMessage()
        {
            RoundTrip(new BridgeErrorMessage("provider failed"), out var parsed);
            parsed
                .Should()
                .BeOfType<BridgeErrorMessage>()
                .Subject.Message.Should()
                .Be("provider failed");
        }

        // --- Negative parsing: explicit errors, never silent null ---

        [TestMethod]
        public void Parse_MalformedJson_ThrowsFormatException()
        {
            Action act = () => BreadcrumbBridgeSerializer.Parse("{not json");
            act.Should().Throw<FormatException>().WithMessage("*Malformed*");
        }

        [TestMethod]
        public void Parse_EmptyOrWhitespace_ThrowsFormatException()
        {
            ((Action)(() => BreadcrumbBridgeSerializer.Parse("")))
                .Should()
                .Throw<FormatException>();
            ((Action)(() => BreadcrumbBridgeSerializer.Parse("   ")))
                .Should()
                .Throw<FormatException>();
        }

        [TestMethod]
        public void Parse_UnknownType_ThrowsFormatException()
        {
            Action act = () => BreadcrumbBridgeSerializer.Parse("{\"type\":\"teleport\"}");
            act.Should().Throw<FormatException>().WithMessage("*teleport*");
        }

        [TestMethod]
        public void Parse_MissingTypeField_ThrowsFormatException()
        {
            Action act = () => BreadcrumbBridgeSerializer.Parse("{\"rowIndex\":1}");
            act.Should().Throw<FormatException>().WithMessage("*'type'*");
        }

        [TestMethod]
        public void Parse_MissingRequiredField_ThrowsFormatException()
        {
            Action act = () =>
                BreadcrumbBridgeSerializer.Parse(
                    "{\"type\":\"segmentDoubleClick\",\"rowIndex\":1}"
                );
            act.Should().Throw<FormatException>().WithMessage("*segmentIndex*");
        }

        [TestMethod]
        public void Parse_UnknownArrowDirection_ThrowsFormatException()
        {
            Action act = () =>
                BreadcrumbBridgeSerializer.Parse("{\"type\":\"arrowKey\",\"direction\":\"up\"}");
            act.Should().Throw<FormatException>().WithMessage("*up*");
        }

        // --- Edge payloads ---

        [TestMethod]
        public void RoundTrip_EmptySubfolderResponse_YieldsEmptyList()
        {
            RoundTrip(
                new SubfolderResponseMessage(3, new BreadcrumbSubfolderRender[0]),
                out var parsed
            );
            parsed
                .Should()
                .BeOfType<SubfolderResponseMessage>()
                .Subject.Subfolders.Should()
                .BeEmpty();
        }

        [TestMethod]
        public void RoundTrip_MaximalLengthFolderPath_SurvivesVerbatim()
        {
            // Outlook full paths max out near 255 chars per segment; stress well past that.
            string longPath = "\\" + string.Join("\\", Enumerable.Repeat(new string('x', 250), 4));
            RoundTrip(new SelectionChangeMessage(0, -1, longPath), out var parsed);
            parsed
                .Should()
                .BeOfType<SelectionChangeMessage>()
                .Subject.SelectedFolder.Should()
                .Be(longPath);
        }

        [TestMethod]
        public void ThemeChangeMessage_EmptyOrWhitespaceTheme_Throws()
        {
            ((Action)(() => new ThemeChangeMessage(""))).Should().Throw<ArgumentException>();
            ((Action)(() => new ThemeChangeMessage("  "))).Should().Throw<ArgumentException>();
            ((Action)(() => new ThemeChangeMessage(null))).Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Serialize_NullMessage_Throws()
        {
            Action act = () => BreadcrumbBridgeSerializer.Serialize(null);
            act.Should().Throw<ArgumentNullException>();
        }

        private static FolderBreadcrumbSegment Segment(
            string entryId,
            string path,
            string name,
            bool hasChildren
        ) =>
            new FolderBreadcrumbSegment(
                new FolderTreeNodeKey("store-a", entryId, path),
                name,
                path,
                hasChildren
            );

        private static FolderBreadcrumbSegment[] SampleChain() =>
            new[]
            {
                Segment("root", "\\Inbox", "Inbox", true),
                Segment("leaf", "\\Inbox\\Léaf", "Léaf", true),
            };
    }
}
