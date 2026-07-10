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
    /// Unit tests for the host-neutral drag/drop move and tree-data logic in the
    /// <c>TaskTreeController.MoveLogic.cs</c> partial. The move methods are exercised directly against
    /// <see cref="ITreeVisual"/> mocks, real <see cref="TreeOfToDoItems"/>/<see cref="TreeNode{T}"/>/
    /// <see cref="ToDoItem"/>/<see cref="IDList"/>, and a recording message seam. No live control is
    /// constructed and no popup is shown.
    /// </summary>
    [TestClass]
    public class TaskTreeControllerMoveLogicTests
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
            List<string> messages = null,
            IDList idList = null
        )
        {
            var viewer = new Mock<ITaskTreeForm>();
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(x => x.IDList).Returns(idList ?? new IDList());
            var globals = new Mock<IApplicationGlobals>();
            globals.Setup(x => x.TD).Returns(mockTD.Object);
            System.Action<string> seam =
                messages == null ? null : new System.Action<string>(messages.Add);
            return new TaskTreeController(globals.Object, viewer.Object, model, seam);
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

        // ---------- MoveObjectsToRoots ----------

        [TestMethod]
        public void MoveObjectsToRoots_SameTree_PromotesChildToRoot()
        {
            // Arrange
            var parent = Node("01");
            var child = Node("0101");
            Attach(parent, child);
            var tree = new Mock<ITreeVisual>();
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));

            // Act — same instance for source and target => same-tree branch
            controller.MoveObjectsToRoots(tree.Object, tree.Object, new ArrayList { child });

            // Assert
            parent.Children.Should().NotContain(child);
            tree.Verify(t => t.AddObject(child), Times.Once);
        }

        [TestMethod]
        public void MoveObjectsToRoots_CrossTree_RemovesFromSourceAddsToTarget()
        {
            // Arrange
            var root = Node("01");
            var source = new Mock<ITreeVisual>();
            var target = new Mock<ITreeVisual>();
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));

            // Act
            controller.MoveObjectsToRoots(target.Object, source.Object, new ArrayList { root });

            // Assert
            root.Parent.Should().BeNull();
            source.Verify(t => t.RemoveObject(root), Times.Once);
            target.Verify(t => t.AddObject(root), Times.Once);
        }

        [TestMethod]
        public void MoveObjectsToRoots_SameTree_AlreadyRoot_DoesNothing()
        {
            // Arrange
            var root = Node("01");
            var tree = new Mock<ITreeVisual>();
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));

            // Act
            controller.MoveObjectsToRoots(tree.Object, tree.Object, new ArrayList { root });

            // Assert
            tree.Verify(t => t.AddObject(It.IsAny<object>()), Times.Never);
        }

        // ---------- MoveObjectsToSibling ----------

        [TestMethod]
        public void MoveObjectsToSibling_RootTarget_RemovesFromRootsAndReseeds()
        {
            // Arrange
            var a = Node("0G");
            var b = Node("0H");
            var moved = Node("ZZ");
            var model = new TreeOfToDoItems(new List<TreeNode<ToDoItem>> { a, b, moved });
            var controller = MakeController(model);

            // Act — target 'a' is a root (Parent null), moved is a root already in Roots.
            // The root-target branch casts toMove to IEnumerable<TreeNode<ToDoItem>>, so a
            // generic list is used (matching the element type the production cast expects).
            controller.MoveObjectsToSibling(
                Mock.Of<ITreeVisual>(),
                Mock.Of<ITreeVisual>(),
                a,
                new List<TreeNode<ToDoItem>> { moved },
                0
            );

            // Assert — moved is reinserted into roots and given a reseeded id
            model.Roots.Should().Contain(moved);
            moved.Value.ToDoID.Should().NotBe("ZZ");
        }

        [TestMethod]
        public void MoveObjectsToSibling_RootNotInRoots_FiresMessageSeam()
        {
            // Arrange — a pad root keeps the post-message reseed index in range
            var pad = Node("0A");
            var target = Node("01");
            var orphan = Node("99"); // Parent null but NOT in model.Roots
            var messages = new List<string>();
            var model = new TreeOfToDoItems(new List<TreeNode<ToDoItem>> { pad, target });
            var controller = MakeController(model, messages);

            // Act — root-target branch casts toMove, so a generic list is used
            controller.MoveObjectsToSibling(
                Mock.Of<ITreeVisual>(),
                Mock.Of<ITreeVisual>(),
                target,
                new List<TreeNode<ToDoItem>> { orphan },
                0
            );

            // Assert
            messages.Should().ContainSingle().Which.Should().Contain("out of sync");
        }

        [TestMethod]
        public void MoveObjectsToSibling_ChildTarget_InsertsIntoParentChildren()
        {
            // Arrange
            var parent = Node("01");
            var target = Node("0101");
            Attach(parent, target);
            var otherParent = Node("02");
            var moved = Node("0201");
            Attach(otherParent, moved);
            var messages = new List<string>();
            var controller = MakeController(
                new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()),
                messages
            );

            // Act — offset 1 => insert after target
            controller.MoveObjectsToSibling(
                Mock.Of<ITreeVisual>(),
                Mock.Of<ITreeVisual>(),
                target,
                new ArrayList { moved },
                1
            );

            // Assert
            parent.Children.Should().Contain(moved);
            moved.Parent.Should().BeSameAs(parent);
            messages.Should().BeEmpty();
        }

        // ---------- MoveObjectsToChildren ----------

        [TestMethod]
        public void MoveObjectsToChildren_RootSource_ReparentsAndRemovesFromRoots()
        {
            // Arrange
            var target = Node("01");
            var moved = Node("99");
            var model = new TreeOfToDoItems(new List<TreeNode<ToDoItem>> { target, moved });
            var source = new Mock<ITreeVisual>();
            var controller = MakeController(model);

            // Act
            controller.MoveObjectsToChildren(
                Mock.Of<ITreeVisual>(),
                source.Object,
                target,
                new ArrayList { moved }
            );

            // Assert
            model.Roots.Should().NotContain(moved);
            moved.Parent.Should().BeSameAs(target);
            source.Verify(t => t.RemoveObject(moved), Times.Once);
        }

        [TestMethod]
        public void MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam()
        {
            // Arrange
            var target = Node("01");
            var orphan = Node("99"); // Parent null but NOT in Roots
            var messages = new List<string>();
            var model = new TreeOfToDoItems(new List<TreeNode<ToDoItem>> { target });
            var source = new Mock<ITreeVisual>();
            var controller = MakeController(model, messages);

            // Act
            controller.MoveObjectsToChildren(
                Mock.Of<ITreeVisual>(),
                source.Object,
                target,
                new ArrayList { orphan }
            );

            // Assert
            messages.Should().ContainSingle().Which.Should().Contain("out of sync");
            source.Verify(t => t.RemoveObject(orphan), Times.Once);
        }

        [TestMethod]
        public void MoveObjectsToChildren_NonRootSource_ReparentsUnderTarget()
        {
            // Arrange
            var target = Node("01");
            var oldParent = Node("02");
            var moved = Node("0201");
            Attach(oldParent, moved);
            var model = new TreeOfToDoItems(new List<TreeNode<ToDoItem>> { target, oldParent });
            var controller = MakeController(model);

            // Act
            controller.MoveObjectsToChildren(
                Mock.Of<ITreeVisual>(),
                Mock.Of<ITreeVisual>(),
                target,
                new ArrayList { moved }
            );

            // Assert
            oldParent.Children.Should().NotContain(moved);
            moved.Parent.Should().BeSameAs(target);
        }

        // ---------- FindChildByID ----------

        [TestMethod]
        public void FindChildByID_FoundAtDepth_ReturnsNode()
        {
            // Arrange
            var root = Node("01");
            var child = Node("0101");
            var grandchild = Node("010101");
            Attach(root, child);
            Attach(child, grandchild);
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));

            // Act
            var result = controller.FindChildByID("010101", new List<TreeNode<ToDoItem>> { root });

            // Assert
            result.Should().BeSameAs(grandchild);
        }

        [TestMethod]
        public void FindChildByID_NotFound_ReturnsNull()
        {
            // Arrange
            var root = Node("01");
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));

            // Act
            var result = controller.FindChildByID("nope", new List<TreeNode<ToDoItem>> { root });

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void FindChildByID_NullId_MatchesEmptyIdNode()
        {
            // Arrange — a node whose id is empty is matched by a null search id (both normalize to "")
            var root = Node("");
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));

            // Act
            var result = controller.FindChildByID(null, new List<TreeNode<ToDoItem>> { root });

            // Assert
            result.Should().BeSameAs(root);
        }

        // ---------- HandleModelCanDrop ----------

        [TestMethod]
        public void HandleModelCanDrop_DropOnSelf_SetsNoneEffect()
        {
            // Arrange
            var self = Node("01");
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));
            var args = DropArgs(self, new ArrayList { self }, DropTargetLocation.Item);

            // Act
            controller.HandleModelCanDrop(null, args);

            // Assert
            args.Effect.Should().Be(System.Windows.Forms.DragDropEffects.None);
            args.InfoMessage.Should().Contain("Cannot drop on self");
        }

        [TestMethod]
        public void HandleModelCanDrop_ReorderAboveItem_SetsMoveEffect()
        {
            // Arrange
            var target = Node("01");
            var source = Node("02");
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));
            var args = DropArgs(target, new ArrayList { source }, DropTargetLocation.AboveItem);

            // Act
            controller.HandleModelCanDrop(null, args);

            // Assert
            args.Effect.Should().Be(System.Windows.Forms.DragDropEffects.Move);
            args.InfoMessage.Should().Contain("reorder");
        }

        [TestMethod]
        public void HandleModelCanDrop_BackgroundAllRoots_SaysAlreadyRoots()
        {
            // Arrange — sources are all roots (Parent null) and both list views are null (same)
            var target = Node("01");
            var source = Node("02");
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));
            var args = DropArgs(target, new ArrayList { source }, DropTargetLocation.Background);

            // Act
            controller.HandleModelCanDrop(null, args);

            // Assert
            args.InfoMessage.Should().Contain("already roots");
        }

        [TestMethod]
        public void HandleModelCanDrop_DropOnDescendant_SaysParadox()
        {
            // Arrange — target is a descendant of the source
            var source = Node("01");
            var target = Node("0101");
            Attach(source, target);
            var controller = MakeController(new TreeOfToDoItems(new List<TreeNode<ToDoItem>>()));
            var args = DropArgs(target, new ArrayList { source }, DropTargetLocation.Item);

            // Act
            controller.HandleModelCanDrop(null, args);

            // Assert
            args.InfoMessage.Should().Contain("descendant");
        }

        // ---------- HandleModelDropped ----------

        [TestMethod]
        public void HandleModelDropped_DefaultLocation_ReturnsEarly()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                new TreeOfToDoItems(new List<TreeNode<ToDoItem>>())
            );
            var args = DropArgs(Node("01"), new ArrayList(), DropTargetLocation.None);

            // Act
            controller.HandleModelDropped(null, args);

            // Assert — early return before the post-drop filter/sort
            viewer.Verify(v => v.SetModelFilter(It.IsAny<System.Predicate<object>>()), Times.Never);
            viewer.Verify(v => v.SortTree(), Times.Never);
        }
    }
}
