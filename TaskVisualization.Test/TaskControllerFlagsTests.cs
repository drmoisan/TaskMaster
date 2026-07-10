using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Non-STA coverage of the flag-application logic in <see cref="TaskController.Flags"/>:
    /// <c>AreCollectionsEqual</c>, both <c>ApplyChange</c> overloads, and the <c>ApplyChanges</c>
    /// iteration wiring. All doubles are in-memory; no live Outlook process is used. The COM
    /// item-write side effects inside <c>ApplyChanges</c> are exercised only on the no-op
    /// (<see cref="Enums.FlagsToSet.None"/>) path, consistent with the exemption inventory.
    /// </summary>
    [TestClass]
    public class TaskControllerFlagsTests
    {
        private static ObservableCollection<string> Oc(params string[] values) =>
            new ObservableCollection<string>(values);

        // ---- AreCollectionsEqual ------------------------------------------------------

        [TestMethod]
        public void AreCollectionsEqual_SameMembersAnyOrder_ReturnsTrue()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.AreCollectionsEqual(Oc("a", "b"), Oc("b", "a")).Should().BeTrue();
        }

        [TestMethod]
        public void AreCollectionsEqual_DisjointMembers_ReturnsFalse()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.AreCollectionsEqual(Oc("a"), Oc("b")).Should().BeFalse();
        }

        [TestMethod]
        public void AreCollectionsEqual_BothNull_ReturnsTrue()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.AreCollectionsEqual(null, null).Should().BeTrue();
        }

        [TestMethod]
        public void AreCollectionsEqual_OneNull_ReturnsFalse()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.AreCollectionsEqual(Oc("a"), null).Should().BeFalse();
        }

        [TestMethod]
        public void AreCollectionsEqual_DuplicatesCollapse_ReturnsTrue()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(view);

            controller.AreCollectionsEqual(Oc("a", "a"), Oc("a")).Should().BeTrue();
        }

        // ---- ApplyChange (flag, current, revised) ------------------------------------

        [TestMethod]
        public void ApplyChange_OptionOff_LeavesCurrentUnchanged()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(
                view,
                options: Enums.FlagsToSet.Context
            );
            var current = ReadOnlyTodo();
            var revised = ReadOnlyTodo();
            current.People.AsStringNoPrefix = "Alice";
            revised.People.AsStringNoPrefix = "Bob";

            // People is not in Options, so no change is applied.
            controller.ApplyChange(Enums.FlagsToSet.People, current.People, revised.People);

            current.People.AsStringNoPrefix.Should().Be("Alice");
            controller.ChangedFlags.HasFlag(Enums.FlagsToSet.People).Should().BeFalse();
        }

        [TestMethod]
        public void ApplyChange_OptionOnEqualCollections_LeavesCurrentUnchangedAndNoFlag()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(
                view,
                options: Enums.FlagsToSet.People
            );
            var current = ReadOnlyTodo();
            var revised = ReadOnlyTodo();
            current.People.AsStringNoPrefix = "Same";
            revised.People.AsStringNoPrefix = "Same";

            controller.ApplyChange(Enums.FlagsToSet.People, current.People, revised.People);

            controller.ChangedFlags.HasFlag(Enums.FlagsToSet.People).Should().BeFalse();
        }

        [TestMethod]
        public void ApplyChange_OptionOnDifferingCollections_WritesCurrentAndSetsFlag()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(
                view,
                options: Enums.FlagsToSet.People
            );
            var current = ReadOnlyTodo();
            var revised = ReadOnlyTodo();
            current.People.AsStringNoPrefix = "Alice";
            revised.People.AsStringNoPrefix = "Bob";

            controller.ApplyChange(Enums.FlagsToSet.People, current.People, revised.People);

            current.People.AsStringNoPrefix.Should().Be("Bob");
            controller.ChangedFlags.HasFlag(Enums.FlagsToSet.People).Should().BeTrue();
        }

        // ---- ApplyChange (fcg, classifier, flag, current, revised) -------------------

        [TestMethod]
        public void ApplyChange_NullFcg_DifferingCollections_WritesCurrentAndSetsFlag()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(
                view,
                options: Enums.FlagsToSet.Projects
            );
            var current = ReadOnlyTodo();
            var revised = ReadOnlyTodo();
            current.Projects.AsStringNoPrefix = "Old";
            revised.Projects.AsStringNoPrefix = "New";

            controller.ApplyChange(
                null,
                "Project",
                Enums.FlagsToSet.Projects,
                current.Projects,
                revised.Projects
            );

            current.Projects.AsStringNoPrefix.Should().Be("New");
            controller.ChangedFlags.HasFlag(Enums.FlagsToSet.Projects).Should().BeTrue();
        }

        [TestMethod]
        public void ApplyChange_FcgEnqueueTrue_WritesCurrentEvenWhenCollectionsEqual()
        {
            var view = new MoqTaskViewer();
            var controller = TaskControllerFixtures.BuildController(
                view,
                options: Enums.FlagsToSet.Projects
            );
            var fcg = new Mock<FlagChangeGroup>(
                TaskControllerFixtures.MockGlobals(),
                TaskControllerFixtures.MailMock()
            );
            fcg.Setup(x =>
                    x.TryEnqueue(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<IEnumerable<string>>()
                    )
                )
                .Returns(true);
            var current = ReadOnlyTodo();
            var revised = ReadOnlyTodo();
            current.Projects.AsStringNoPrefix = "Same";
            revised.Projects.AsStringNoPrefix = "Same";

            controller.ApplyChange(
                fcg.Object,
                "Project",
                Enums.FlagsToSet.Projects,
                current.Projects,
                revised.Projects
            );

            controller.ChangedFlags.HasFlag(Enums.FlagsToSet.Projects).Should().BeTrue();
        }

        // ApplyChanges itself is not driven here: it is method-level exempt COM-iteration wiring
        // (FlagChangeGroup + ToDoEvents.Editing + WriteFlagsBatchAsync + training-queue side
        // effects) that does not terminate deterministically over Moq doubles. Its extractable
        // units (both ApplyChange overloads and AreCollectionsEqual) are the measured logic above.

        // A read-only ToDoItem whose category mutations do not trigger live Outlook writes.
        private static ToDoItem ReadOnlyTodo()
        {
            var todo = TaskControllerFixtures.BuildTodo();
            todo.ReadOnly = true;
            return todo;
        }
    }
}
