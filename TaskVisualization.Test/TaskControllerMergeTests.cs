using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Non-STA coverage of the merge helpers (<c>MergeToCollection</c>, <c>MergeFlag</c>) and
    /// <c>AutoAssignAllAsync</c> on <see cref="TaskController"/>. All doubles are in-memory; no live
    /// form, popup, or Outlook process is used.
    /// </summary>
    [TestClass]
    public class TaskControllerMergeTests
    {
        [TestMethod]
        public void MergeToCollection_UnionsWithoutDuplicates()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            var original = new ObservableCollection<string> { "a", "b" };

            var merged = controller.MergeToCollection(original, new List<string> { "b", "c" });

            merged.Should().BeEquivalentTo(new[] { "a", "b", "c" });
        }

        [TestMethod]
        public void MergeFlag_NullList_IsNoOp()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            var before = controller.Active.People.AsStringNoPrefix;

            controller.MergeFlag(null, Enums.FlagsToSet.People);

            controller.Active.People.AsStringNoPrefix.Should().Be(before);
        }

        [TestMethod]
        public void MergeFlag_EmptyList_IsNoOp()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            var before = controller.Active.People.AsStringNoPrefix;

            controller.MergeFlag(new List<string>(), Enums.FlagsToSet.People);

            controller.Active.People.AsStringNoPrefix.Should().Be(before);
        }

        [TestMethod]
        public void MergeFlag_People_MergesIntoActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.MergeFlag(new List<string> { "Bob" }, Enums.FlagsToSet.People);

            controller.Active.People.AsStringNoPrefix.Should().Contain("Bob");
            view.Mock.VerifySet(v => v.PeopleText = It.IsAny<string>(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void MergeFlag_Context_MergesIntoActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.MergeFlag(new List<string> { "Calls" }, Enums.FlagsToSet.Context);

            controller.Active.Context.AsStringNoPrefix.Should().Contain("Calls");
            view.Mock.VerifySet(v => v.ContextText = It.IsAny<string>(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void MergeFlag_Projects_MergesIntoActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.MergeFlag(new List<string> { "Apollo" }, Enums.FlagsToSet.Projects);

            controller.Active.Projects.AsStringNoPrefix.Should().Contain("Apollo");
            view.Mock.VerifySet(v => v.ProjectText = It.IsAny<string>(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void MergeFlag_Topics_MergesIntoActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.MergeFlag(new List<string> { "Budget" }, Enums.FlagsToSet.Topics);

            controller.Active.Topics.AsStringNoPrefix.Should().Contain("Budget");
            view.Mock.VerifySet(v => v.TopicText = It.IsAny<string>(), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task AutoAssignAllAsync_MailWithPeopleTags_MergesPeople()
        {
            // The injected people auto-assigner returns a tag; the mail-helper factory returns a
            // completed task (no live Outlook), so the mail branch runs end to end.
            var view = new MoqTaskViewer();
            var people = TaskControllerFixtures.AutoAssign(new List<string> { "Dave" });
            var controller = TaskControllerFixtures.BuildController(view, autoAssign: people);

            await controller.AutoAssignAllAsync();

            controller.Active.People.AsStringNoPrefix.Should().Contain("Dave");
        }

        [TestMethod]
        public async Task AutoAssignAllAsync_MailWithProjectAndContextTags_MergesBoth()
        {
            // Exercises the ProjectAssign and ContextAssign merge branches of AutoAssignAllAsync.
            var view = new MoqTaskViewer();
            var project = TaskControllerFixtures.AutoAssign(new List<string> { "Apollo" });
            var context = TaskControllerFixtures.AutoAssign(new List<string> { "Calls" });
            var controller = TaskControllerFixtures.BuildController(
                view,
                projectAssign: project,
                contextAssign: context
            );

            await controller.AutoAssignAllAsync();

            controller.Active.Projects.AsStringNoPrefix.Should().Contain("Apollo");
            controller.Active.Context.AsStringNoPrefix.Should().Contain("Calls");
        }

        [TestMethod]
        public async Task AutoAssignAllAsync_MailWithNoTags_LeavesCategoriesUnchanged()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            var before = controller.Active.People.AsStringNoPrefix;

            await controller.AutoAssignAllAsync();

            controller.Active.People.AsStringNoPrefix.Should().Be(before);
        }
    }
}
