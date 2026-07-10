using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tags.Test.Fakes;
using UtilitiesCS;

namespace Tags.Test
{
    /// <summary>
    /// Seam-driven coverage for <see cref="TagController"/> orchestration: dialog-routed methods via
    /// a Moq <see cref="IUserPrompt"/>, keyboard and navigation handlers against a
    /// <see cref="FakeTagViewer"/> with a no-op draw seam, and property forwarders. No live form,
    /// popup, or HWND is used.
    /// </summary>
    [TestClass]
    public class TagControllerSeamTests
    {
        [TestMethod]
        public void ResolveMailItem_ReturnsMailForMailItemAndNullOtherwise()
        {
            // NOTE (report-only, per spec Non-Goals): ResolveMailItem branches on the parameter but
            // returns the cast _objItem field, so it is constructed here with the mail as _objItem to
            // exercise the three parameter branches against its real usage without altering behavior.
            var viewer = new FakeTagViewer();
            var mail = new Mock<MailItem>().Object;
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string>());
            var controller = BuildWithAutoAssign(
                viewer,
                new Mock<IUserPrompt>(MockBehavior.Loose).Object,
                autoAssigner.Object,
                mail
            );

            controller.ResolveMailItem(mail).Should().BeSameAs(mail);
            controller.ResolveMailItem("not a mail").Should().BeNull();
            controller.ResolveMailItem(null).Should().BeNull();

            DisposeOptions(viewer);
        }

        [TestMethod]
        public void GetUserInputCategory_WithPrefilledName_RoutesThroughPrompt()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .Setup(p =>
                    p.GetCategoryInput(
                        "The following category name will be added:",
                        "Add Category Dialog",
                        "Existing"
                    )
                )
                .Returns("Edited");
            var controller = BuildSimple(viewer, prompt.Object);

            controller.GetUserInputCategory("Existing").Should().Be("Edited");

            DisposeOptions(viewer);
        }

        [TestMethod]
        public void GetUserInputCategory_WithEmptyName_LoopsUntilNonSpaceResponse()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .SetupSequence(p =>
                    p.GetCategoryInput(It.IsAny<string>(), "Add Category Dialog", " ")
                )
                .Returns(" ")
                .Returns("Chosen");
            var controller = BuildSimple(viewer, prompt.Object);

            controller.GetUserInputCategory("").Should().Be("Chosen");
            prompt.Verify(
                p => p.GetCategoryInput(It.IsAny<string>(), "Add Category Dialog", " "),
                Times.Exactly(2)
            );

            DisposeOptions(viewer);
        }

        [TestMethod]
        public void AddColorCategory_WithUserInput_AddsOptionAndFiltersToSelected()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .Setup(p => p.GetCategoryInput(It.IsAny<string>(), It.IsAny<string>(), "Cat"))
                .Returns("Cat");
            var controller = BuildSimple(viewer, prompt.Object);

            controller.AddColorCategory("Cat");

            controller.GetSelections().Should().Equal("Cat");
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void AddColorCategory_WithEmptyInput_DoesNotAddOption()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .Setup(p => p.GetCategoryInput(It.IsAny<string>(), It.IsAny<string>(), " "))
                .Returns("");
            var controller = BuildSimple(viewer, prompt.Object);

            controller.AddColorCategory("");

            controller.GetSelections().Should().BeEmpty();
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void AddColorCategory_WhenAutoAssignerReturnsNullCategory_ReturnsEarly()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .Setup(p => p.GetCategoryInput(It.IsAny<string>(), It.IsAny<string>(), "Cat"))
                .Returns("Cat");
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string>());
            autoAssigner
                .Setup(x => x.AddColorCategory(It.IsAny<IPrefix>(), "Cat"))
                .Returns((Category)null);
            var controller = BuildWithAutoAssign(viewer, prompt.Object, autoAssigner.Object, null);

            controller.AddColorCategory("Cat");

            controller.GetSelections().Should().BeEmpty();
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void TryGetAutoAssignment_WithMailAndYes_AddsAssignmentsAndReturnsTrue()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .Setup(p => p.ShowYesNo("Auto-add new from email details?", "Dialog"))
                .Returns(DialogResult.Yes);
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string>());
            autoAssigner
                .Setup(x =>
                    x.AddChoicesToDict(
                        It.IsAny<MailItem>(),
                        It.IsAny<IList<IPrefix>>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    )
                )
                .Returns(new List<string> { "New1", "New2" });
            var controller = BuildWithAutoAssign(
                viewer,
                prompt.Object,
                autoAssigner.Object,
                new Mock<MailItem>().Object
            );

            controller.TryGetAutoAssignment(out var assignments).Should().BeTrue();

            assignments.Should().Equal("New1", "New2");
            controller.GetSelections().Should().Contain(new[] { "New1", "New2" });
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void TryGetAutoAssignment_WhenUserDeclines_ReturnsFalse()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .Setup(p => p.ShowYesNo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(DialogResult.No);
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string>());
            var controller = BuildWithAutoAssign(
                viewer,
                prompt.Object,
                autoAssigner.Object,
                new Mock<MailItem>().Object
            );

            controller.TryGetAutoAssignment(out _).Should().BeFalse();
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void TryGetAutoAssignment_WhenNotMail_ReturnsFalseWithoutPrompting()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string>());
            var controller = BuildWithAutoAssign(viewer, prompt.Object, autoAssigner.Object, null);

            controller.TryGetAutoAssignment(out _).Should().BeFalse();
            prompt.Verify(p => p.ShowYesNo(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void LoadSelections_WhenKeyMissingAndUserAgrees_RoutesToAddColorCategory()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .Setup(p => p.ShowYesNo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(DialogResult.Yes);
            prompt
                .Setup(p => p.GetCategoryInput(It.IsAny<string>(), It.IsAny<string>(), "Missing"))
                .Returns("");

            var controller = new TagController(
                viewer.Object,
                new SortedDictionary<string, bool>(),
                new List<string> { "Missing" },
                null,
                prompt.Object,
                _ => { }
            );

            prompt.Verify(p => p.ShowYesNo(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            DisposeOptions(viewer);
        }

        [TestMethod]
        public void LoadSelections_WhenKeyMissingAndUserDeclines_DoesNotPromptForCategory()
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            prompt
                .Setup(p => p.ShowYesNo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(DialogResult.No);

            var controller = new TagController(
                viewer.Object,
                new SortedDictionary<string, bool>(),
                new List<string> { "Missing" },
                null,
                prompt.Object,
                _ => { }
            );

            controller.GetSelections().Should().BeEmpty();
            prompt.Verify(
                p => p.GetCategoryInput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
            DisposeOptions(viewer);
        }

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

        [TestMethod]
        public void PropertyForwarders_RouteToViewer()
        {
            var viewer = new FakeTagViewer();
            var controller = BuildSimple(viewer);

            controller.SetCaption("Title");
            viewer.Caption.Should().Be("Title");

            controller.SetSearchText("query");
            viewer.SearchTextValue.Should().Be("query");

            controller.ButtonNewActive = true;
            controller.ButtonNewActive.Should().BeTrue();
            viewer.ButtonNewVisible.Should().BeTrue();
        }

        [TestMethod]
        public void SetAutoAssignState_TogglesViewerButtonVisibilityByMailAndAssigner()
        {
            var viewer = new FakeTagViewer();
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string>());
            var controller = BuildWithAutoAssign(
                viewer,
                new Mock<IUserPrompt>().Object,
                autoAssigner.Object,
                new Mock<MailItem>().Object
            );

            // _isMail is true in the auto-assign fixture, so a non-null assigner enables the button.
            controller.SetAutoAssignState(autoAssigner.Object);
            viewer.AutoAssignVisible.Should().BeTrue();
            viewer.AutoAssignEnabled.Should().BeTrue();

            controller.SetAutoAssignState(null);
            viewer.AutoAssignVisible.Should().BeFalse();
            viewer.AutoAssignEnabled.Should().BeFalse();
            DisposeOptions(viewer);
        }

        private static TagController BuildSimple(
            FakeTagViewer viewer,
            IUserPrompt prompt = null,
            SortedDictionary<string, bool> options = null
        )
        {
            return new TagController(
                viewer.Object,
                options ?? new SortedDictionary<string, bool>(),
                null,
                NewPrefix("Program", "TagProgram "),
                prompt ?? new Mock<IUserPrompt>(MockBehavior.Loose).Object,
                _ => { }
            );
        }

        private static TagController BuildWithAutoAssign(
            FakeTagViewer viewer,
            IUserPrompt prompt,
            IAutoAssign autoAssigner,
            MailItem mailItem
        )
        {
            var controller = new TagController(
                viewer.Object,
                new SortedDictionary<string, bool>(),
                autoAssigner,
                new List<IPrefix> { NewPrefix("Program", "TagProgram ") },
                "user@example.test",
                prefixKey: "Program",
                objItemObject: mailItem,
                prompt: prompt,
                drawFocus: _ => { }
            );
            if (mailItem != null)
            {
                SetPrivateField(controller, "_isMail", true);
                controller.SetAutoAssignState(autoAssigner);
            }
            return controller;
        }

        private static IPrefix NewPrefix(string key, string value)
        {
            var prefix = new Mock<IPrefix>(MockBehavior.Loose);
            prefix.SetupGet(p => p.Key).Returns(key);
            prefix.SetupGet(p => p.Value).Returns(value);
            return prefix.Object;
        }

        private static void DisposeOptions(FakeTagViewer viewer)
        {
            foreach (var control in viewer.OptionControls.ToList())
            {
                control.Dispose();
            }
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target
                .GetType()
                .GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );
            field.Should().NotBeNull();
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );
            field.Should().NotBeNull();
            field.SetValue(target, value);
        }
    }
}
