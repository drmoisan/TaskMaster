using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Conversation-view coverage for <see cref="QfcExplorerController"/>: the
    /// <c>ExplConvView_ToggleOn</c> and <c>ExplConvView_ToggleOff</c> state transitions and the
    /// <c>GetSiblingView</c> lookup. Second part of the partial class declared in
    /// <c>QfcExplorerControllerTests.cs</c>, split to keep both files under the 500-line limit.
    /// <c>[TestClass]</c> stays on the base file only: it is <c>AllowMultiple = false</c>, so
    /// repeating it here would be CS0579. Shares the base file's <c>Setup</c> fixture,
    /// <c>CreateController</c>, and <c>ArrangeViewsIndexer</c> helpers. Deterministic — mocked COM
    /// only, no live form, no message pump, no sleeps, no temporary files.
    /// </summary>
    public partial class QfcExplorerControllerTests
    {
        /// <summary>
        /// <c>ExplConvView_ToggleOn</c> applies the remembered view and clears the flag when
        /// <c>BlShowInConversations</c> is set.
        /// </summary>
        [TestMethod]
        public void ExplConvView_ToggleOn_WhenFlagSet_AppliesRememberedView()
        {
            // Arrange
            var rememberedView = _repo.Create<Outlook.View>();
            var views = ArrangeViewsIndexer(rememberedView);
            var currentFolder = _repo.Create<MAPIFolder>();
            currentFolder.SetupGet(f => f.Views).Returns(views.Object);
            _explorer.SetupGet(e => e.CurrentFolder).Returns(currentFolder.Object);

            var controller = CreateController();
            controller.BlShowInConversations = true;

            // Act
            controller.ExplConvView_ToggleOn();

            // Assert
            rememberedView.Verify(v => v.Apply(), Times.Once());
            controller
                .BlShowInConversations.Should()
                .BeFalse("applying the remembered view completes the toggle-on transition");
        }

        /// <summary>
        /// <c>ExplConvView_ToggleOn</c> is a no-op when <c>BlShowInConversations</c> is clear: the
        /// negative branch must not touch the <c>Views</c> collection or apply any view.
        /// </summary>
        [TestMethod]
        public void ExplConvView_ToggleOn_WhenFlagClear_DoesNothing()
        {
            // Arrange
            var rememberedView = _repo.Create<Outlook.View>();
            var views = ArrangeViewsIndexer(rememberedView);
            var currentFolder = _repo.Create<MAPIFolder>();
            currentFolder.SetupGet(f => f.Views).Returns(views.Object);
            _explorer.SetupGet(e => e.CurrentFolder).Returns(currentFolder.Object);

            var controller = CreateController();
            controller.BlShowInConversations = false;

            // Act
            controller.ExplConvView_ToggleOn();

            // Assert
            views.Verify(v => v[It.IsAny<object>()], Times.Never());
            rememberedView.Verify(v => v.Apply(), Times.Never());
            controller.BlShowInConversations.Should().BeFalse();
        }

        /// <summary>
        /// <c>ExplConvView_ToggleOff</c> is a no-op when conversations are not grouped: the guard reads
        /// the command-bar state and returns without touching the current view.
        /// </summary>
        [TestMethod]
        public void ExplConvView_ToggleOff_WhenConversationsNotGrouped_DoesNothing()
        {
            // Arrange — the fixture already arranges GetPressedMso to return false.
            var controller = CreateController();
            controller.BlShowInConversations = false;

            // Act
            controller.ExplConvView_ToggleOff();

            // Assert
            controller
                .BlShowInConversations.Should()
                .BeFalse("the negative guard must leave the flag untouched");
            _explorer.VerifyGet(e => e.CurrentView, Times.Never());
        }

        /// <summary>
        /// <c>ExplConvView_ToggleOff</c> copies the current view to a temporary conversation-free view,
        /// strips the upgrade-to-conversation directive from its XML, saves it, and applies it, when no
        /// sibling view of that name already exists.
        /// </summary>
        [TestMethod]
        public void ExplConvView_ToggleOff_WhenSiblingViewMissing_CopiesAndSavesTemporaryView()
        {
            // Arrange — conversations ARE grouped, so the branch is entered.
            _commandBars.Setup(c => c.GetPressedMso(ShowInConversationsMso)).Returns(true);

            // A sibling collection with no view named tmpNoConversation, so GetSiblingView returns null.
            var siblings = _repo.Create<Views>();
            siblings
                .Setup(v => v.GetEnumerator())
                .Returns(() => ((IEnumerable)new List<Outlook.View>()).GetEnumerator());

            var currentView = _repo.Create<Outlook.View>();
            currentView.SetupGet(v => v.Name).Returns("Compact");
            currentView.SetupGet(v => v.Parent).Returns(siblings.Object);
            currentView
                .SetupGet(v => v.XML)
                .Returns("<view><upgradetoconv>1</upgradetoconv></view>");
            _explorer.SetupGet(e => e.CurrentView).Returns(currentView.Object);

            var temporaryView = _repo.Create<Outlook.View>();
            currentView
                .Setup(v =>
                    v.Copy(TemporaryViewName, OlViewSaveOption.olViewSaveOptionThisFolderOnlyMe)
                )
                .Returns(temporaryView.Object);

            var controller = CreateController();

            // Act
            controller.ExplConvView_ToggleOff();

            // Assert — copy, XML assignment with the directive stripped, save, then apply.
            currentView.Verify(
                v => v.Copy(TemporaryViewName, OlViewSaveOption.olViewSaveOptionThisFolderOnlyMe),
                Times.Once()
            );
            temporaryView.VerifySet(v => v.XML = "<view></view>", Times.Once());
            temporaryView.Verify(v => v.Save(), Times.Once());
            temporaryView.Verify(v => v.Apply(), Times.Once());
            controller
                .BlShowInConversations.Should()
                .BeTrue("toggling off records that conversations were grouped");
        }

        /// <summary>
        /// <c>GetSiblingView</c> returns the sibling whose name matches the requested view name.
        /// </summary>
        [TestMethod]
        public void GetSiblingView_WhenNamedViewPresent_ReturnsIt()
        {
            // Arrange
            var other = _repo.Create<Outlook.View>();
            other.SetupGet(v => v.Name).Returns("Compact");
            var wanted = _repo.Create<Outlook.View>();
            wanted.SetupGet(v => v.Name).Returns(TemporaryViewName);

            var siblings = _repo.Create<Views>();
            var list = new List<Outlook.View> { other.Object, wanted.Object };
            siblings
                .Setup(v => v.GetEnumerator())
                .Returns(() => ((IEnumerable)list).GetEnumerator());

            var currentView = _repo.Create<Outlook.View>();
            currentView.SetupGet(v => v.Parent).Returns(siblings.Object);

            var controller = CreateController();

            // Act
            Outlook.View result = controller.GetSiblingView(currentView.Object, TemporaryViewName);

            // Assert
            result.Should().BeSameAs(wanted.Object);
        }

        /// <summary>
        /// <c>GetSiblingView</c> returns null when the loop is exhausted without a name match.
        /// </summary>
        [TestMethod]
        public void GetSiblingView_WhenNamedViewAbsent_ReturnsNull()
        {
            // Arrange
            var other = _repo.Create<Outlook.View>();
            other.SetupGet(v => v.Name).Returns("Compact");

            var siblings = _repo.Create<Views>();
            var list = new List<Outlook.View> { other.Object };
            siblings
                .Setup(v => v.GetEnumerator())
                .Returns(() => ((IEnumerable)list).GetEnumerator());

            var currentView = _repo.Create<Outlook.View>();
            currentView.SetupGet(v => v.Parent).Returns(siblings.Object);

            var controller = CreateController();

            // Act
            Outlook.View result = controller.GetSiblingView(currentView.Object, TemporaryViewName);

            // Assert
            result.Should().BeNull();
        }
    }
}
