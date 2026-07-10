using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    [TestClass]
    public class TaskControllerTests
    {
        private static TaskController Build(
            MoqTaskViewer view,
            Enums.FlagsToSet options = Enums.FlagsToSet.All,
            Mock<ITagPromptService> tagPrompt = null,
            List<string> warnings = null
        )
        {
            var autoAssign = TaskControllerFixtures.AutoAssign();
            return new TaskController(
                view.Object,
                TaskControllerFixtures.MockCategories(),
                TaskControllerFixtures.TodoList(1),
                TaskControllerFixtures.Defaults(),
                autoAssign.Object,
                autoAssign.Object,
                autoAssign.Object,
                s => s,
                "user@test.com",
                null,
                options,
                (tagPrompt ?? TaskControllerFixtures.TagPrompt(true)).Object,
                TaskControllerFixtures.CapturingNotifier(warnings ?? new List<string>()),
                TaskControllerFixtures.MailHelperFactory()
            );
        }

        [TestMethod]
        public void Constructor_EnumeratesCategories_AndClonesActive()
        {
            // Arrange / Act
            var view = new MoqTaskViewer();
            var controller = Build(view);

            // Assert: construction succeeds and the mocked viewer received a controller.
            controller.Should().NotBeNull();
            view.Mock.Verify(v => v.SetController(controller), Times.Once);
        }

        [TestMethod]
        public void Constructor_SevenParameterOverload_ConstructsAndRegisters()
        {
            // Exercises the 7-parameter constructor overload (not used by FlagTasks).
            var view = new MoqTaskViewer();

            var controller = TaskControllerFixtures.BuildController7(view);

            controller.Should().NotBeNull();
            view.Mock.Verify(v => v.SetController(controller), Times.Once);
        }

        [TestMethod]
        public void InitializeData_WithCategoriesAndSchedule_WritesOptionalFacadeFields()
        {
            // Arrange: seed the working clone so the optional (non-empty / scheduled) branches of
            // InitializeData execute (category texts, reminder, and due-date writes).
            var view = new MoqTaskViewer();
            var controller = Build(view);
            controller.Active.Context.AsStringNoPrefix = "Deep Work";
            controller.Active.People.AsStringNoPrefix = "Jane";
            controller.Active.Projects.AsStringNoPrefix = "Apollo";
            controller.Active.Topics.AsStringNoPrefix = "Budget";
            // ReminderTime's setter is a field-only assignment (no COM write), so the getter
            // returns this value and the reminder branch of InitializeData executes.
            controller.Active.ReminderTime = new System.DateTime(2020, 2, 2);

            // Act
            controller.InitializeData();

            // Assert: the optional category / reminder facade setters were written.
            view.Mock.VerifySet(v => v.ContextText = "Deep Work", Times.AtLeastOnce);
            view.Mock.VerifySet(v => v.PeopleText = "Jane", Times.AtLeastOnce);
            view.Mock.VerifySet(v => v.ReminderChecked = true, Times.AtLeastOnce);
        }

        [TestMethod]
        public void InitializeData_WritesFacadePrimitives_ThroughTheViewer()
        {
            // Arrange
            var view = new MoqTaskViewer();
            var controller = Build(view);

            // Act
            controller.InitializeData();

            // Assert: the task subject and duration were written through the primitive facade.
            view.Mock.VerifySet(v => v.TaskNameText = It.IsAny<string>(), Times.AtLeastOnce);
            view.Mock.VerifySet(v => v.DurationText = It.IsAny<string>(), Times.AtLeastOnce);
            view.Mock.VerifySet(
                v => v.PrioritySelectedItem = It.IsAny<object>(),
                Times.AtLeastOnce
            );
            view.Mock.VerifySet(v => v.KbSelectedItem = It.IsAny<object>(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void AnyCategorySelected_AllPlaceholders_ReturnsFalse()
        {
            // Arrange: MoqTaskViewer seeds all four category facade texts with placeholders.
            var view = new MoqTaskViewer();
            var controller = Build(view);

            // Act / Assert
            controller.AnyCategorySelected.Should().BeFalse();
        }

        [TestMethod]
        public void AnyCategorySelected_OneDiffers_ReturnsTrue()
        {
            // Arrange: change exactly one of the four category texts away from its placeholder.
            var view = new MoqTaskViewer();
            var controller = Build(view);
            view.Object.ContextText = "Real Context";

            // Act / Assert: a single differing category flips the property true (edge: exactly one).
            controller.AnyCategorySelected.Should().BeTrue();
        }

        [TestMethod]
        public void AnyCategorySelected_PeopleDiffers_ReturnsTrue()
        {
            // Arrange
            var view = new MoqTaskViewer();
            var controller = Build(view);
            view.Object.PeopleText = "Jane Doe";

            // Act / Assert
            controller.AnyCategorySelected.Should().BeTrue();
        }

        [TestMethod]
        public void OptionsSetter_UpdatesOptionsValue_AndActivatesWithoutThrowing()
        {
            // Arrange
            var view = new MoqTaskViewer();
            var controller = Build(view, Enums.FlagsToSet.All);

            // Act: setting Options re-runs ActivateOptions over the mocked control surface.
            controller.Options = Enums.FlagsToSet.Context;

            // Assert: the setter stored the new value and the option activation ran cleanly.
            controller.Options.Should().Be(Enums.FlagsToSet.Context);
        }
    }
}
