using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tags.Test.Fakes;

namespace Tags.Test
{
    /// <summary>
    /// Seam-driven coverage for <see cref="TagController"/> keyboard and navigation handlers
    /// (option-panel key routing, search-text key handling, and page up/down scrolling) against a
    /// <see cref="FakeTagViewer"/> with a no-op draw seam. No live form, popup, or HWND is used.
    /// </summary>
    /// <remarks>
    /// This is a partial-class continuation of <see cref="TagControllerSeamTests"/>; see
    /// <c>TagControllerSeamTests.cs</c> for the dialog-routed, auto-assign, and property-forwarder
    /// coverage plus the shared test-construction helpers used by both files.
    /// </remarks>
    public partial class TagControllerSeamTests
    {
        [TestMethod]
        public void OptionsPanelPreviewKeyDown_MarksUpAndDownAsInputKeys()
        {
            var viewer = new FakeTagViewer();
            var controller = BuildSimple(viewer);

            var down = new PreviewKeyDownEventArgs(Keys.Down);
            var up = new PreviewKeyDownEventArgs(Keys.Up);
            var other = new PreviewKeyDownEventArgs(Keys.Left);

            controller.OptionsPanel_PreviewKeyDown(null, down);
            controller.OptionsPanel_PreviewKeyDown(null, up);
            controller.OptionsPanel_PreviewKeyDown(null, other);

            down.IsInputKey.Should().BeTrue();
            up.IsInputKey.Should().BeTrue();
            other.IsInputKey.Should().BeFalse();
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void OptionsPanelKeyDown_MovesFocusUpAndDown()
        {
            var viewer = new FakeTagViewer();
            var controller = BuildSimple(
                viewer,
                options: new SortedDictionary<string, bool> { ["A"] = false, ["B"] = false }
            );

            controller.OptionsPanel_KeyDown(null, new KeyEventArgs(Keys.Down));
            GetPrivateField<int>(controller, "intFocus").Should().Be(0);

            controller.OptionsPanel_KeyDown(null, new KeyEventArgs(Keys.Down));
            GetPrivateField<int>(controller, "intFocus").Should().Be(1);

            controller.OptionsPanel_KeyDown(null, new KeyEventArgs(Keys.Up));
            GetPrivateField<int>(controller, "intFocus").Should().Be(0);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void TagViewerKeyDown_OnEnter_TriggersOkExit()
        {
            var viewer = new FakeTagViewer();
            var controller = BuildSimple(viewer);

            controller.TagViewer_KeyDown(null, new KeyEventArgs(Keys.Enter));

            controller.ExitType.Should().Be("Normal");
            viewer.Mock.Verify(v => v.Close(), Times.Once);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void SearchTextKeyDownAndKeyUp_RecordCursorAndFilterToSelected()
        {
            var viewer = new FakeTagViewer();
            viewer.SearchSelectionStart = 3;
            var controller = BuildSimple(
                viewer,
                options: new SortedDictionary<string, bool> { ["A"] = true, ["B"] = false }
            );

            controller.SearchText_KeyDown(null, new KeyEventArgs(Keys.Right));
            controller.SearchText_KeyUp(null, new KeyEventArgs(Keys.Right));

            // Right-KeyUp at the same cursor position filters to the selected option only.
            viewer.OptionControls.Select(c => c.Tag as string).Should().Equal("A");
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void SearchTextKeyDown_OnDown_MovesFocusToFirstOption()
        {
            var viewer = new FakeTagViewer();
            var controller = BuildSimple(
                viewer,
                options: new SortedDictionary<string, bool> { ["A"] = false }
            );

            controller.SearchText_KeyDown(null, new KeyEventArgs(Keys.Down));

            GetPrivateField<int>(controller, "intFocus").Should().Be(0);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void SearchTextKeyUp_OnEnter_TriggersOkExit()
        {
            var viewer = new FakeTagViewer();
            var controller = BuildSimple(viewer);

            controller.SearchText_KeyUp(null, new KeyEventArgs(Keys.Enter));

            controller.ExitType.Should().Be("Normal");
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void SelectPageDown_WhenScrollFits_DoesNothing()
        {
            var viewer = new FakeTagViewer { OptionsScrollMaximum = 10, OptionsPanelHeight = 100 };
            var controller = BuildSimple(
                viewer,
                options: new SortedDictionary<string, bool> { ["A"] = false, ["B"] = false }
            );

            controller.Select_PageDown();

            GetPrivateField<int>(controller, "intFocus").Should().Be(-1);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void SelectPageDown_WhenNoRowBelowViewport_SelectsLastControl()
        {
            var viewer = new FakeTagViewer
            {
                OptionsScrollMaximum = 1000,
                OptionsPanelHeight = 100,
            };
            var controller = BuildSimple(
                viewer,
                options: new SortedDictionary<string, bool> { ["A"] = false, ["B"] = false }
            );

            controller.Select_PageDown();

            GetPrivateField<int>(controller, "intFocus").Should().Be(1);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void SelectPageDown_WhenRowBelowViewport_ScrollsToIt()
        {
            var viewer = new FakeTagViewer
            {
                OptionsScrollMaximum = 1000,
                OptionsPanelHeight = 100,
            };
            var controller = BuildSimple(
                viewer,
                options: new SortedDictionary<string, bool> { ["A"] = false, ["B"] = false }
            );
            viewer.OptionControls[1].Top = 200;
            viewer.OptionControls[1].Height = 50;

            controller.Select_PageDown();

            GetPrivateField<int>(controller, "intFocus").Should().Be(1);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void SelectPageUp_WhenRowAboveViewport_ScrollsToIt()
        {
            var viewer = new FakeTagViewer
            {
                OptionsScrollMaximum = 1000,
                OptionsPanelHeight = 100,
            };
            var controller = BuildSimple(
                viewer,
                options: new SortedDictionary<string, bool> { ["A"] = false, ["B"] = false }
            );
            viewer.OptionControls[0].Top = -50;

            controller.Select_PageUp();

            GetPrivateField<int>(controller, "intFocus").Should().Be(0);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void SelectPageUp_WhenNoRowAboveViewport_SelectsFirstControl()
        {
            var viewer = new FakeTagViewer
            {
                OptionsScrollMaximum = 1000,
                OptionsPanelHeight = 100,
            };
            var controller = BuildSimple(
                viewer,
                options: new SortedDictionary<string, bool> { ["A"] = false, ["B"] = false }
            );

            controller.Select_PageUp();

            GetPrivateField<int>(controller, "intFocus").Should().Be(0);
            DisposeOptions(viewer);
        }
    }
}
