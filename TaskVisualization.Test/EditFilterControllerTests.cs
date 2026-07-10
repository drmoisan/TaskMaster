using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for <see cref="EditFilterController"/> driven through a
    /// <see cref="Mock{IEditFilterViewer}"/> injected via the viewer-factory seam and
    /// a stubbed tag-selector seam. No live form is constructed and no popup is shown.
    /// </summary>
    [TestClass]
    public class EditFilterControllerTests
    {
        private static EditFilterController Build(
            Mock<IEditFilterViewer> viewer,
            FilterEntry filterEntry = null,
            Action<EditFilterController, FilterEntry> callback = null,
            Func<SortedDictionary<string, bool>, (bool cancelled, string selection)> tagSelector =
                null
        )
        {
            var globals = new MoqOlToDo().MockGlobals();
            return new EditFilterController(
                globals,
                filterEntry,
                callback,
                () => viewer.Object,
                tagSelector ?? (d => (true, null))
            );
        }

        [TestMethod]
        public void Initialize_SetsSelectionTextsFromFlags_AndResetsTips()
        {
            var viewer = new Mock<IEditFilterViewer>();
            var filterEntry = new FilterEntry();
            filterEntry.Flags.Context.AsStringNoPrefix = "Ctx";
            filterEntry.Flags.People.AsStringNoPrefix = "Ppl";

            Build(viewer, filterEntry, (c, fe) => { });

            viewer.VerifySet(v => v.ContextSelectionText = "Ctx", Times.Once());
            viewer.VerifySet(v => v.PeopleSelectionText = "Ppl", Times.Once());
            viewer.Verify(v => v.ResetTips(), Times.Once);
        }

        [TestMethod]
        public void InitializeFactory_UsesInjectedViewer_AndSetsProjectTopicTexts()
        {
            var viewer = new Mock<IEditFilterViewer>();
            var filterEntry = new FilterEntry();
            filterEntry.Flags.Projects.AsStringNoPrefix = "Prj";
            filterEntry.Flags.Topics.AsStringNoPrefix = "Top";

            Build(viewer, filterEntry, (c, fe) => { });

            viewer.VerifySet(v => v.ProjectSelectionText = "Prj", Times.Once());
            viewer.VerifySet(v => v.TopicSelectionText = "Top", Times.Once());
        }

        [TestMethod]
        public void InitializeFactory_ReturnsInjectedViewer_AndAppliesSelectionText()
        {
            var viewer = new Mock<IEditFilterViewer>();
            var filterEntry = new FilterEntry();
            filterEntry.Flags.Context.AsStringNoPrefix = "C2";
            var globals = new MoqOlToDo().MockGlobals();
            var controller = new EditFilterController(
                globals,
                filterEntry,
                null,
                () => viewer.Object,
                d => (true, null)
            );

            var result = controller.InitializeFactory();

            result.Should().BeSameAs(viewer.Object);
            viewer.VerifySet(v => v.ContextSelectionText = "C2");
        }

        [TestMethod]
        public void OkClick_AddPath_NullFilterEntry_CommitsFreshEntry()
        {
            // Add-filter path: null entry -> core ctor creates a fresh FilterEntry.
            var viewer = new Mock<IEditFilterViewer>();
            viewer.Setup(v => v.FilterNameText).Returns("Fresh");
            FilterEntry committed = null;
            Build(viewer, filterEntry: null, callback: (c, fe) => committed = fe);

            viewer.Raise(v => v.OkClick += null, viewer.Object, EventArgs.Empty);

            committed.Should().NotBeNull();
            committed.Name.Should().Be("Fresh");
        }

        [TestMethod]
        public void OkClick_WithCallback_SetsName_InvokesCallback_HidesAndDisposes()
        {
            var viewer = new Mock<IEditFilterViewer>();
            viewer.Setup(v => v.FilterNameText).Returns("MyFilter");
            var filterEntry = new FilterEntry();
            EditFilterController cbController = null;
            FilterEntry cbEntry = null;
            var controller = Build(
                viewer,
                filterEntry,
                (c, fe) =>
                {
                    cbController = c;
                    cbEntry = fe;
                }
            );

            viewer.Raise(v => v.OkClick += null, viewer.Object, EventArgs.Empty);

            filterEntry.Name.Should().Be("MyFilter");
            cbController.Should().BeSameAs(controller);
            cbEntry.Should().BeSameAs(filterEntry);
            viewer.Verify(v => v.Hide(), Times.Once);
            viewer.Verify(v => v.Dispose(), Times.Once);
        }

        [TestMethod]
        public void CancelClick_NoCallback_ClosesAndRevertsToCopy()
        {
            var viewer = new Mock<IEditFilterViewer>();
            var filterEntry = new FilterEntry { Name = "Original" };
            Build(viewer, filterEntry, callback: null);
            filterEntry.Name = "Modified";

            viewer.Raise(v => v.CancelClick += null, viewer.Object, EventArgs.Empty);

            viewer.Verify(v => v.Close(), Times.Once);
            filterEntry.Name.Should().Be("Original");
        }

        [TestMethod]
        public void CancelClick_WithCallback_DoesNotClose()
        {
            var viewer = new Mock<IEditFilterViewer>();
            Build(viewer, new FilterEntry(), (c, fe) => { });

            viewer.Raise(v => v.CancelClick += null, viewer.Object, EventArgs.Empty);

            viewer.Verify(v => v.Close(), Times.Never);
        }

        [TestMethod]
        public void ContextSelectionClick_TagSelectorConfirms_WritesSelectionText()
        {
            var viewer = new Mock<IEditFilterViewer>();
            var filterEntry = new FilterEntry();
            Build(viewer, filterEntry, (c, fe) => { }, tagSelector: d => (false, "X;Y"));

            viewer.Raise(v => v.ContextSelectionClick += null, viewer.Object, EventArgs.Empty);

            viewer.VerifySet(v => v.ContextSelectionText = "X;Y", Times.Once());
            filterEntry.Flags.Context.AsStringNoPrefix.Should().Be("X;Y");
        }

        [TestMethod]
        public void PeopleProjectTopicClicks_TagSelectorConfirms_WriteRespectiveText()
        {
            var viewer = new Mock<IEditFilterViewer>();
            Build(viewer, new FilterEntry(), (c, fe) => { }, tagSelector: d => (false, "Z"));

            viewer.Raise(v => v.PeopleSelectionClick += null, viewer.Object, EventArgs.Empty);
            viewer.Raise(v => v.ProjectSelectionClick += null, viewer.Object, EventArgs.Empty);
            viewer.Raise(v => v.TopicSelectionClick += null, viewer.Object, EventArgs.Empty);
            // Folders click handler is intentionally a no-op; it must not throw.
            viewer.Raise(v => v.FoldersSelectedClick += null, viewer.Object, EventArgs.Empty);

            viewer.VerifySet(v => v.PeopleSelectionText = "Z", Times.Once());
            viewer.VerifySet(v => v.ProjectSelectionText = "Z", Times.Once());
            viewer.VerifySet(v => v.TopicSelectionText = "Z", Times.Once());
        }

        [TestMethod]
        public void ContextSelectionClick_TagSelectorCancels_DoesNotWrite()
        {
            var viewer = new Mock<IEditFilterViewer>();
            Build(viewer, new FilterEntry(), (c, fe) => { }, tagSelector: d => (true, null));
            viewer.Invocations.Clear();

            viewer.Raise(v => v.ContextSelectionClick += null, viewer.Object, EventArgs.Empty);

            viewer.VerifySet(v => v.ContextSelectionText = It.IsAny<string>(), Times.Never());
        }
    }
}
