using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskTree.Test
{
    /// <summary>
    /// Unit tests for the host-neutral coordination logic in <see cref="TaskTreeController"/> (the
    /// main partial). All UI interaction is exercised through a mocked <see cref="ITaskTreeForm"/>
    /// facade and mocked Outlook Interop interfaces; no live <c>Form</c>/<c>Control</c> is created and
    /// no popup is shown.
    /// </summary>
    [TestClass]
    public class TaskTreeControllerTests
    {
        private static TreeOfToDoItems ModelWithRoots(params string[] ids)
        {
            var roots = new List<TreeNode<ToDoItem>>();
            foreach (var id in ids)
            {
                roots.Add(new TreeNode<ToDoItem>(new ToDoItem(id) { ReadOnly = true }));
            }
            return new TreeOfToDoItems(roots);
        }

        private static TreeNode<ToDoItem> NodeWithInner(object inner)
        {
            var mockInner = new Mock<IOutlookItem>();
            mockInner.Setup(x => x.InnerObject).Returns(inner);
            var todo = new ToDoItem(mockInner.Object, onDemand: true);
            return new TreeNode<ToDoItem>(todo);
        }

        // ---------- Constructor ----------

        [TestMethod]
        public void Constructor_AssignsControllerOnViewer()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();

            // Act
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Assert
            viewer.Verify(v => v.SetController(controller), Times.Once);
        }

        [TestMethod]
        public void Constructor_NullMessageSeam_DoesNotThrow()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();

            // Act
            System.Action act = () =>
                new TaskTreeController(
                    globals.Object,
                    viewer.Object,
                    ModelWithRoots(),
                    showMessage: null
                );

            // Assert
            act.Should().NotThrow();
        }

        // ---------- InitializeTreeListView ----------

        [TestMethod]
        public void InitializeTreeListView_InvokesFacadeWithRootsAndFilter()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var model = ModelWithRoots("01", "02");
            var controller = new TaskTreeController(globals.Object, viewer.Object, model);

            // Act
            controller.InitializeTreeListView();

            // Assert
            viewer.Verify(
                v => v.InitializeTreeView(model.Roots, It.Is<Predicate<object>>(p => p != null)),
                Times.Once
            );
            viewer.Verify(v => v.ResizeControls(), Times.Once);
        }

        // ---------- Toggle expand/collapse ----------

        [TestMethod]
        public void ToggleExpandCollapseAll_AlternatesExpandThenCollapse()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            controller.ToggleExpandCollapseAll();
            controller.ToggleExpandCollapseAll();

            // Assert
            viewer.Verify(v => v.ExpandAllNodes(), Times.Once);
            viewer.Verify(v => v.CollapseAllNodes(), Times.Once);
        }

        // ---------- Toggle hide complete ----------

        [TestMethod]
        public void ToggleHideComplete_AlternatesClearThenSetFilter()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            controller.ToggleHideComplete();
            controller.ToggleHideComplete();

            // Assert
            viewer.Verify(v => v.SetModelFilter(null), Times.Once);
            viewer.Verify(
                v => v.SetModelFilter(It.Is<Predicate<object>>(p => p != null)),
                Times.Once
            );
        }

        // ---------- Rebuild ----------

        [TestMethod]
        public void RebuildTreeVisual_InvokesRebuildTreeWithRoots()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var model = ModelWithRoots("01");
            var controller = new TaskTreeController(globals.Object, viewer.Object, model);

            // Act
            controller.RebuildTreeVisual();

            // Assert
            viewer.Verify(v => v.RebuildTree(model.Roots), Times.Once);
        }

        // ---------- Resize ----------

        [TestMethod]
        public void ResizeForm_InvokesResizeControlsAndAutoSize()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            controller.ResizeForm();

            // Assert
            viewer.Verify(v => v.ResizeControls(), Times.Once);
            viewer.Verify(v => v.AutoSizeTreeColumns(), Times.Once);
        }

        // ---------- GetSelectedTreeNode ----------

        [TestMethod]
        public void GetSelectedTreeNode_ReturnsFacadeSelection()
        {
            // Arrange
            var node = new TreeNode<ToDoItem>(new ToDoItem("01") { ReadOnly = true });
            var viewer = new Mock<ITaskTreeForm>();
            viewer.Setup(v => v.GetSelectedNode()).Returns(node);
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            var result = controller.GetSelectedTreeNode();

            // Assert
            result.Should().BeSameAs(node);
        }

        [TestMethod]
        public void GetSelectedTreeNode_WhenNothingSelected_ReturnsNull()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            viewer.Setup(v => v.GetSelectedNode()).Returns((TreeNode<ToDoItem>)null);
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            var result = controller.GetSelectedTreeNode();

            // Assert
            result.Should().BeNull();
        }

        // ---------- ResolveRowStyle (extracted strikeout decision) ----------

        [TestMethod]
        public void ResolveRowStyle_WhenComplete_AddsStrikeout()
        {
            // Act
            var style = TaskTreeController.ResolveRowStyle(FontStyle.Regular, complete: true);

            // Assert
            style.HasFlag(FontStyle.Strikeout).Should().BeTrue();
        }

        [TestMethod]
        public void ResolveRowStyle_WhenNotComplete_RemovesStrikeout()
        {
            // Act
            var style = TaskTreeController.ResolveRowStyle(FontStyle.Strikeout, complete: false);

            // Assert
            style.HasFlag(FontStyle.Strikeout).Should().BeFalse();
        }

        // ---------- IsValidType ----------

        [TestMethod]
        public void IsValidType_MailItem_ReturnsTrue()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act / Assert
            controller.IsValidType(new Mock<MailItem>().Object).Should().BeTrue();
        }

        [TestMethod]
        public void IsValidType_TaskItem_ReturnsTrue()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act / Assert
            controller.IsValidType(new Mock<TaskItem>().Object).Should().BeTrue();
        }

        [TestMethod]
        public void IsValidType_OtherObject_ReturnsFalse()
        {
            // Arrange
            var viewer = new Mock<ITaskTreeForm>();
            var globals = new Mock<IApplicationGlobals>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act / Assert
            controller.IsValidType(new object()).Should().BeFalse();
        }

        // ---------- ActivateOlItem / ActivateOlItemAsync (null-guard) ----------
        //
        // ActivateOlItem(Async) now take an `object` item (not `dynamic`), so the Explorer selection and
        // typed-Display branches bind statically against the mockable Outlook interop interfaces. The
        // null-guard is exercised here; the selectable / not-selectable / Display branches are covered
        // in TaskTreeControllerActivateTests.cs against a mocked Explorer.

        [TestMethod]
        public void ActivateOlItem_WhenItemNull_NoExplorerInteraction()
        {
            // Arrange
            var globals = new Mock<IApplicationGlobals>();
            var viewer = new Mock<ITaskTreeForm>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            controller.ActivateOlItem(null);

            // Assert
            globals.Verify(g => g.Ol, Times.Never);
        }

        [TestMethod]
        public async Task ActivateOlItemAsync_WhenItemNull_NoExplorerInteraction()
        {
            // Arrange
            var globals = new Mock<IApplicationGlobals>();
            var viewer = new Mock<ITaskTreeForm>();
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            await controller.ActivateOlItemAsync(null);

            // Assert
            globals.Verify(g => g.Ol, Times.Never);
        }

        // ---------- TreeLvActivateItem (sync wrapper) ----------

        [TestMethod]
        public void TreeLvActivateItem_WhenNoSelection_IsNoOp()
        {
            // Arrange
            var globals = new Mock<IApplicationGlobals>();
            var viewer = new Mock<ITaskTreeForm>();
            viewer.Setup(v => v.GetSelectedNode()).Returns((TreeNode<ToDoItem>)null);
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            controller.TreeLvActivateItem();

            // Assert
            globals.Verify(g => g.Ol, Times.Never);
        }

        [TestMethod]
        public void TreeLvActivateItem_WhenUnsupportedType_FiresMessageSeam()
        {
            // Arrange
            var messages = new List<string>();
            var globals = new Mock<IApplicationGlobals>();
            var viewer = new Mock<ITaskTreeForm>();
            viewer
                .Setup(v => v.GetSelectedNode())
                .Returns(NodeWithInner(new Mock<ContactItem>().Object));
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots(),
                messages.Add
            );

            // Act
            controller.TreeLvActivateItem();

            // Assert
            messages.Should().ContainSingle().Which.Should().Contain("Unsupported type");
            globals.Verify(g => g.Ol, Times.Never);
        }

        // ---------- TreeLvActivateItemAsync (async wrapper) ----------

        [TestMethod]
        public async Task TreeLvActivateItemAsync_WhenNoSelection_IsNoOp()
        {
            // Arrange
            var globals = new Mock<IApplicationGlobals>();
            var viewer = new Mock<ITaskTreeForm>();
            viewer.Setup(v => v.GetSelectedNode()).Returns((TreeNode<ToDoItem>)null);
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots()
            );

            // Act
            await controller.TreeLvActivateItemAsync();

            // Assert
            globals.Verify(g => g.Ol, Times.Never);
        }

        [TestMethod]
        public async Task TreeLvActivateItemAsync_WhenUnsupportedType_FiresMessageSeam()
        {
            // Arrange
            var messages = new List<string>();
            var globals = new Mock<IApplicationGlobals>();
            var viewer = new Mock<ITaskTreeForm>();
            viewer
                .Setup(v => v.GetSelectedNode())
                .Returns(NodeWithInner(new Mock<ContactItem>().Object));
            var controller = new TaskTreeController(
                globals.Object,
                viewer.Object,
                ModelWithRoots(),
                messages.Add
            );

            // Act
            await controller.TreeLvActivateItemAsync();

            // Assert
            messages.Should().ContainSingle().Which.Should().Contain("Unsupported type");
        }
    }
}
