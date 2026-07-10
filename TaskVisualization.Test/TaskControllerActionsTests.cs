using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Non-STA coverage of the seam-reducible action / flag-mutation methods in
    /// <see cref="TaskController.Actions"/>. Every dependency is a mocked
    /// <see cref="ITaskViewer"/> plus injected seams; no live form, popup, Outlook process, or
    /// temp file is created, and no time-derived value is asserted.
    /// </summary>
    [TestClass]
    public class TaskControllerActionsTests
    {
        // ---- SetFlag ------------------------------------------------------------------

        [TestMethod]
        public void SetFlag_Context_UpdatesActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.SetFlag("Meeting", Enums.FlagsToSet.Context);

            controller.Active.Context.AsStringNoPrefix.Should().Be("Meeting");
            view.Mock.VerifySet(v => v.ContextText = "Meeting", Times.AtLeastOnce);
        }

        [TestMethod]
        public void SetFlag_People_UpdatesActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.SetFlag("Alice", Enums.FlagsToSet.People);

            controller.Active.People.AsStringNoPrefix.Should().Be("Alice");
            view.Mock.VerifySet(v => v.PeopleText = "Alice", Times.AtLeastOnce);
        }

        [TestMethod]
        public void SetFlag_Projects_UpdatesActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.SetFlag("Apollo", Enums.FlagsToSet.Projects);

            controller.Active.Projects.AsStringNoPrefix.Should().Be("Apollo");
            view.Mock.VerifySet(v => v.ProjectText = "Apollo", Times.AtLeastOnce);
        }

        [TestMethod]
        public void SetFlag_Topics_UpdatesActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.SetFlag("Budget", Enums.FlagsToSet.Topics);

            controller.Active.Topics.AsStringNoPrefix.Should().Be("Budget");
            view.Mock.VerifySet(v => v.TopicText = "Budget", Times.AtLeastOnce);
        }

        [TestMethod]
        public void SetFlag_Taskname_WritesSubjectAndFacade()
        {
            // Arrange: the direct _active.TaskSubject write is COM-bound over a Moq MailItem, so
            // the controller routes the Taskname branch through the injected
            // setActiveTaskSubject seam. Capture what the seam receives instead of writing to a
            // live Outlook-interop property.
            var view = new MoqTaskViewer();
            var captured = new List<string>();
            var controller = TaskControllerFixtures.BuildController(
                view,
                setActiveTaskSubject: v => captured.Add(v)
            );

            controller.SetFlag("New Subject", Enums.FlagsToSet.Taskname);

            captured.Should().Equal(new[] { "New Subject" });
            view.Mock.VerifySet(v => v.TaskNameText = "New Subject", Times.AtLeastOnce);
        }

        [TestMethod]
        public void SetFlag_Worktime_WritesDurationFacadeOnly()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.SetFlag("30", Enums.FlagsToSet.Worktime);

            view.Mock.VerifySet(v => v.DurationText = "30", Times.AtLeastOnce);
        }

        // ---- Shortcuts ----------------------------------------------------------------

        [TestMethod]
        public void Shortcut_Meeting_SetsContext()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Shortcut_Meeting();

            controller.Active.Context.AsStringNoPrefix.Should().Be("Meeting");
        }

        [TestMethod]
        public void Shortcut_Email_SetsContext()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Shortcut_Email();

            controller.Active.Context.AsStringNoPrefix.Should().Be("Email");
        }

        [TestMethod]
        public void Shortcut_Calls_SetsContext()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Shortcut_Calls();

            controller.Active.Context.AsStringNoPrefix.Should().Be("Calls");
        }

        [TestMethod]
        public void Shortcut_PreRead_SetsContext()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Shortcut_PreRead();

            controller.Active.Context.AsStringNoPrefix.Should().Be("PreRead");
        }

        [TestMethod]
        public void Shortcut_WaitingFor_SetsContext()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Shortcut_WaitingFor();

            controller.Active.Context.AsStringNoPrefix.Should().Be("Waiting For");
        }

        [TestMethod]
        public void Shortcut_Unprocessed_SetsContext()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Shortcut_Unprocessed();

            controller
                .Active.Context.AsStringNoPrefix.Should()
                .Be("Reading - .Unprocessed > 2 Minutes");
        }

        [TestMethod]
        public void Shortcut_ReadingBusiness_SetsContext()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Shortcut_ReadingBusiness();

            controller.Active.Context.AsStringNoPrefix.Should().Be("Reading - Business");
        }

        [TestMethod]
        public void Shortcut_Personal_SetsContextAndProject()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Shortcut_Personal();

            controller.Active.Context.AsStringNoPrefix.Should().Contain("Personal");
            controller.Active.Projects.AsStringNoPrefix.Should().Contain("Personal - Other");
        }

        [TestMethod]
        public void Shortcut_ReadingNews_SetsAllFlagsAndFocusesDuration()
        {
            // Arrange: exercises the Context/Projects/Taskname/Worktime SetFlag chain plus the
            // FocusDuration facade call. The Taskname write is captured via the injected
            // setActiveTaskSubject seam rather than the COM-bound _active.TaskSubject setter.
            var view = new MoqTaskViewer();
            view.Object.TaskNameText = "Original Subject";
            var captured = new List<string>();
            var controller = TaskControllerFixtures.BuildController(
                view,
                setActiveTaskSubject: v => captured.Add(v)
            );

            controller.Shortcut_ReadingNews();

            controller
                .Active.Context.AsStringNoPrefix.Should()
                .Be("Reading - News | Articles | Other");
            controller.Active.Projects.AsStringNoPrefix.Should().Be("Routine - Reading");
            captured.Should().Equal(new[] { "READ: Original Subject" });
            view.Mock.VerifySet(v => v.DurationText = "15", Times.AtLeastOnce);
            view.Mock.Verify(v => v.FocusDuration(), Times.Once);
        }

        // ---- Assign_* / *_Change ------------------------------------------------------

        [TestMethod]
        public void Assign_KB_CopiesFacadeSelectionToActive()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.KbSelectedItem = "Backlog";

            controller.Assign_KB();

            controller.Active.KB.AsStringNoPrefix.Should().Be("Backlog");
        }

        [TestMethod]
        public void Assign_Priority_High_MapsToHighImportance()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.PrioritySelectedItem = "High";

            controller.Assign_Priority();

            controller.Active.Priority.Should().Be(OlImportance.olImportanceHigh);
        }

        [TestMethod]
        public void Assign_Priority_Low_MapsToLowImportance()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.PrioritySelectedItem = "Low";

            controller.Assign_Priority();

            controller.Active.Priority.Should().Be(OlImportance.olImportanceLow);
        }

        [TestMethod]
        public void Assign_Priority_Unknown_FallsBackToNormal()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.PrioritySelectedItem = "Nonsense";

            controller.Assign_Priority();

            controller.Active.Priority.Should().Be(OlImportance.olImportanceNormal);
        }

        [TestMethod]
        public void Today_Change_CopiesFacadeCheckedToActive()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.TodayChecked = true;

            controller.Today_Change();

            controller.Active.Today.Should().BeTrue();
        }

        [TestMethod]
        public void Bullpin_Change_CopiesFacadeCheckedToActive()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.BullpinChecked = true;

            controller.Bullpin_Change();

            controller.Active.Bullpin.Should().BeTrue();
        }

        [TestMethod]
        public void FlagAsTask_Change_CopiesFacadeCheckedToActive()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.FlagAsTaskChecked = true;

            controller.FlagAsTask_Change();

            controller.Active.FlagAsTask.Should().BeTrue();
        }

        // ---- Assign dialogs (ITagPromptService seam) ---------------------------------

        [TestMethod]
        public void AssignPeople_Selection_UpdatesActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var prompt = TaskControllerFixtures.TagPrompt(cancelled: false, selection: "Carol");
            var controller = TaskControllerFixtures.BuildController(view, tagPrompt: prompt);

            controller.AssignPeople();

            controller.Active.People.AsStringNoPrefix.Should().Be("Carol");
            view.Mock.VerifySet(v => v.PeopleText = "Carol", Times.AtLeastOnce);
        }

        [TestMethod]
        public void AssignPeople_Cancel_LeavesStateUnchanged()
        {
            var view = new MoqTaskViewer();
            var prompt = TaskControllerFixtures.TagPrompt(cancelled: true, selection: "Ignored");
            var controller = TaskControllerFixtures.BuildController(view, tagPrompt: prompt);
            var before = controller.Active.People.AsStringNoPrefix;

            controller.AssignPeople();

            controller.Active.People.AsStringNoPrefix.Should().Be(before);
        }

        [TestMethod]
        public void AssignContext_Selection_UpdatesActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var prompt = TaskControllerFixtures.TagPrompt(cancelled: false, selection: "Deep Work");
            var controller = TaskControllerFixtures.BuildController(view, tagPrompt: prompt);

            controller.AssignContext();

            controller.Active.Context.AsStringNoPrefix.Should().Be("Deep Work");
            view.Mock.VerifySet(v => v.ContextText = "Deep Work", Times.AtLeastOnce);
        }

        [TestMethod]
        public void AssignProject_Selection_UpdatesActiveFacadeAndProgram()
        {
            var view = new MoqTaskViewer();
            var prompt = TaskControllerFixtures.TagPrompt(cancelled: false, selection: "Orion");
            var controller = TaskControllerFixtures.BuildController(view, tagPrompt: prompt);

            controller.AssignProject();

            controller.Active.Projects.AsStringNoPrefix.Should().Be("Orion");
            // ProjectsToPrograms is the identity map in the fixture, so Program mirrors Projects.
            controller.Active.Program.AsStringNoPrefix.Should().Be("Orion");
            view.Mock.VerifySet(v => v.ProjectText = "Orion", Times.AtLeastOnce);
        }

        [TestMethod]
        public void AssignTopic_Selection_UpdatesActiveAndFacade()
        {
            var view = new MoqTaskViewer();
            var prompt = TaskControllerFixtures.TagPrompt(cancelled: false, selection: "Roadmap");
            var controller = TaskControllerFixtures.BuildController(view, tagPrompt: prompt);

            controller.AssignTopic();

            controller.Active.Topics.AsStringNoPrefix.Should().Be("Roadmap");
            view.Mock.VerifySet(v => v.TopicText = "Roadmap", Times.AtLeastOnce);
        }

        // ---- CaptureDuration ----------------------------------------------------------

        [TestMethod]
        public void CaptureDuration_ValidInteger_SetsTotalWork()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.DurationText = "45";

            controller.CaptureDuration();

            controller.Active.TotalWork.Should().Be(45);
        }

        [TestMethod]
        public void CaptureDuration_Negative_InvokesNotifierAndLeavesTotalWorkUnchanged()
        {
            var view = new MoqTaskViewer();
            var warnings = new List<string>();
            var controller = TaskControllerFixtures.BuildController(view, warnings: warnings);
            var before = controller.Active.TotalWork;
            view.Object.DurationText = "-5";

            controller.CaptureDuration();

            warnings.Should().ContainSingle();
            controller.Active.TotalWork.Should().Be(before);
        }

        [TestMethod]
        public void CaptureDuration_NonInteger_PropagatesFormatException()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);
            view.Object.DurationText = "abc";

            System.Action act = () => controller.CaptureDuration();

            act.Should().Throw<FormatException>();
        }

        // ---- OK_Action / Cancel_Action -----------------------------------------------

        [TestMethod]
        public async Task OK_Action_NoCategorySelected_DoesNotApplyOrSetDialogResult()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            await controller.OK_Action();

            view.Mock.Verify(v => v.Hide(), Times.Never);
            view.Mock.VerifySet(v => v.DialogResult = It.IsAny<DialogResult>(), Times.Never);
        }

        // OK_Action's category-selected branch is not driven here: after CaptureDuration and Hide
        // it awaits ApplyChanges, the method-level exempt COM-iteration wiring that does not
        // terminate deterministically over Moq doubles. The AnyCategorySelected == false decision
        // (and the InvokeRequired == false inline path) are covered by the test above.

        [TestMethod]
        public void Cancel_Action_HidesSetsCancelAndDisposes()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.Cancel_Action();

            view.Mock.Verify(v => v.Hide(), Times.Once);
            view.Mock.VerifySet(v => v.DialogResult = DialogResult.Cancel, Times.Once);
            view.Mock.Verify(v => v.Dispose(), Times.Once);
        }
    }
}
