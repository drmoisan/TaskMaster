using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel;
using UtilitiesCS;

namespace TaskTree.Test
{
    /// <summary>
    /// Unit tests for the host-neutral drop routing extracted from the control-bound
    /// <c>HandleModelDropped</c> wrapper into <see cref="TaskTreeController.RouteDrop"/> and
    /// <see cref="TaskTreeController.ApplyPostDropView"/>. Each <see cref="DropTargetLocation"/> branch is
    /// exercised directly against <see cref="ITreeVisual"/> mocks and a test-constructed
    /// <see cref="ModelDropEventArgs"/>; no live control is constructed and no popup is shown.
    /// </summary>
    [TestClass]
    public class TaskTreeControllerRouteDropTests
    {
        private static TreeNode<ToDoItem> Node(string id) =>
            new TreeNode<ToDoItem>(new ToDoItem(id) { ReadOnly = true });

        private static void Attach(TreeNode<ToDoItem> parent, TreeNode<ToDoItem> child)
        {
            child.Parent = parent;
            parent.Children.Add(child);
        }

        private static TaskTreeController MakeController(
            TreeOfToDoItems model,
            Mock<ITaskTreeForm> viewer = null,
            List<string> messages = null
        )
        {
            var v = viewer ?? new Mock<ITaskTreeForm>();
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(x => x.IDList).Returns(new IDList());
            var globals = new Mock<IApplicationGlobals>();
            globals.Setup(x => x.TD).Returns(mockTD.Object);
            System.Action<string> seam =
                messages == null ? null : new System.Action<string>(messages.Add);
            return new TaskTreeController(globals.Object, v.Object, model, seam);
        }

        private static ModelDropEventArgs DropArgs(
            object target,
            IList sources,
            DropTargetLocation location
        )
        {
            var args = new ModelDropEventArgs();
            typeof(ModelDropEventArgs)
                .GetField("targetModel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(args, target);
            typeof(ModelDropEventArgs)
                .GetField("dragModels", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(args, new ArrayList(sources ?? new ArrayList()));
            args.DropTargetLocation = location;
            return args;
        }

        // ---------- RouteDrop: Background ----------

        [TestMethod]
        public void RouteDrop_Background_RoutesToRoots_ReturnsTrue()
        {
            // Arrange — cross-tree background drop of a root node
            var root = Node("01");
            var source = new Mock<ITreeVisual>();
            var target = new Mock<ITreeVisual>();
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));
            var args = DropArgs(Node("02"), new ArrayList { root }, DropTargetLocation.Background);

            // Act
            var routed = controller.RouteDrop(target.Object, source.Object, args);

            // Assert
            routed.Should().BeTrue();
            source.Verify(t => t.RemoveObject(root), Times.Once);
            target.Verify(t => t.AddObject(root), Times.Once);
        }

        // ---------- RouteDrop: Item ----------

        [TestMethod]
        public void RouteDrop_Item_RoutesToChildren_ReturnsTrue()
        {
            // Arrange — drop a root node onto a target item to make it a child
            var target = Node("01");
            var moved = Node("99");
            var model = new TreeOfToDoItems(new List<TreeNode<ToDoItem>> { target, moved });
            var source = new Mock<ITreeVisual>();
            var controller = MakeController(model);
            var args = DropArgs(target, new ArrayList { moved }, DropTargetLocation.Item);

            // Act
            var routed = controller.RouteDrop(Mock.Of<ITreeVisual>(), source.Object, args);

            // Assert
            routed.Should().BeTrue();
            moved.Parent.Should().BeSameAs(target);
            model.Roots.Should().NotContain(moved);
            source.Verify(t => t.RemoveObject(moved), Times.Once);
        }

        // ---------- RouteDrop: AboveItem (sibling offset 0) ----------

        [TestMethod]
        public void RouteDrop_AboveItem_InsertsSiblingBeforeTarget_ReturnsTrue()
        {
            // Arrange
            var parent = Node("01");
            var target = Node("0101");
            Attach(parent, target);
            var otherParent = Node("02");
            var moved = Node("0201");
            Attach(otherParent, moved);
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));
            var args = DropArgs(target, new ArrayList { moved }, DropTargetLocation.AboveItem);

            // Act — offset 0 inserts the moved node before the target
            var routed = controller.RouteDrop(Mock.Of<ITreeVisual>(), Mock.Of<ITreeVisual>(), args);

            // Assert
            routed.Should().BeTrue();
            parent.Children.Should().Contain(moved);
            parent.Children.IndexOf(moved).Should().BeLessThan(parent.Children.IndexOf(target));
        }

        // ---------- RouteDrop: BelowItem (sibling offset 1) ----------

        [TestMethod]
        public void RouteDrop_BelowItem_InsertsSiblingAfterTarget_ReturnsTrue()
        {
            // Arrange
            var parent = Node("01");
            var target = Node("0101");
            Attach(parent, target);
            var otherParent = Node("02");
            var moved = Node("0201");
            Attach(otherParent, moved);
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));
            var args = DropArgs(target, new ArrayList { moved }, DropTargetLocation.BelowItem);

            // Act — offset 1 inserts the moved node after the target
            var routed = controller.RouteDrop(Mock.Of<ITreeVisual>(), Mock.Of<ITreeVisual>(), args);

            // Assert
            routed.Should().BeTrue();
            parent.Children.Should().Contain(moved);
            parent.Children.IndexOf(moved).Should().BeGreaterThan(parent.Children.IndexOf(target));
        }

        // ---------- RouteDrop: default (unhandled) ----------

        [TestMethod]
        public void RouteDrop_UnhandledLocation_ReturnsFalse_NoMove()
        {
            // Arrange
            var target = new Mock<ITreeVisual>();
            var source = new Mock<ITreeVisual>();
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));
            var args = DropArgs(Node("01"), new ArrayList(), DropTargetLocation.None);

            // Act
            var routed = controller.RouteDrop(target.Object, source.Object, args);

            // Assert
            routed.Should().BeFalse();
            target.Verify(t => t.AddObject(It.IsAny<object>()), Times.Never);
            source.Verify(t => t.RemoveObject(It.IsAny<object>()), Times.Never);
        }

        // ---------- ApplyPostDropView ----------

        [TestMethod]
        public void ApplyPostDropView_WhenFilterActive_ReAppliesFilterAndSorts()
        {
            // Arrange — a fresh controller starts with the incomplete filter active
            var viewer = new Mock<ITaskTreeForm>();
            var controller = MakeController(
                new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()),
                viewer
            );

            // Act
            controller.ApplyPostDropView();

            // Assert
            viewer.Verify(
                v => v.SetModelFilter(It.Is<System.Predicate<object>>(p => p != null)),
                Times.Once
            );
            viewer.Verify(v => v.SortTree(), Times.Once);
        }

        [TestMethod]
        public void ApplyPostDropView_WhenFilterInactive_SortsWithoutReapplyingFilter()
        {
            // Arrange — toggle the hide-complete filter off before applying the post-drop view
            var viewer = new Mock<ITaskTreeForm>();
            var controller = MakeController(
                new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()),
                viewer
            );
            controller.ToggleHideComplete(); // clears the filter (_filterCompleted => false)
            viewer.Invocations.Clear();

            // Act
            controller.ApplyPostDropView();

            // Assert
            viewer.Verify(v => v.SetModelFilter(It.IsAny<System.Predicate<object>>()), Times.Never);
            viewer.Verify(v => v.SortTree(), Times.Once);
        }
    }
}
