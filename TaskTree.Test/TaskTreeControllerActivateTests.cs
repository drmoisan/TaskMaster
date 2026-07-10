using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel;
using UtilitiesCS;

namespace TaskTree.Test
{
    /// <summary>
    /// Unit tests for the Outlook Explorer activation seam on <see cref="TaskTreeController"/>. Because
    /// <see cref="TaskTreeController.ActivateOlItem"/> / <c>ActivateOlItemAsync</c> now accept an
    /// <see cref="object"/> item (not <c>dynamic</c>), the selectable / not-selectable / typed-Display
    /// branches bind statically and are exercised here against a mocked <see cref="Explorer"/>. The
    /// caller valid-type paths (<see cref="TaskTreeController.TreeLvActivateItem"/> /
    /// <c>TreeLvActivateItemAsync</c>) are also covered. No live control or popup is created.
    /// </summary>
    [TestClass]
    public class TaskTreeControllerActivateTests
    {
        private static TreeOfToDoItems EmptyModel() =>
            new TreeOfToDoItems(new List<TreeNode<ToDoItem>>());

        private static TreeNode<ToDoItem> NodeWithInner(object inner)
        {
            var mockInner = new Mock<IOutlookItem>();
            mockInner.Setup(x => x.InnerObject).Returns(inner);
            var todo = new ToDoItem(mockInner.Object, onDemand: true);
            return new TreeNode<ToDoItem>(todo);
        }

        private static Mock<IApplicationGlobals> GlobalsWithExplorer(Mock<Explorer> explorer)
        {
            var app = new Mock<Application>();
            app.Setup(a => a.ActiveExplorer()).Returns(explorer.Object);
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.Ol.App).Returns(app.Object);
            return globals;
        }

        private static TaskTreeController MakeController(Mock<IApplicationGlobals> globals)
        {
            var viewer = new Mock<ITaskTreeForm>();
            return new TaskTreeController(globals.Object, viewer.Object, EmptyModel());
        }

        // ---------- ActivateOlItem (sync) ----------

        [TestMethod]
        public void ActivateOlItem_WhenSelectable_ClearsThenAddsToSelection()
        {
            // Arrange
            var mail = new Mock<MailItem>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(true);
            var controller = MakeController(GlobalsWithExplorer(explorer));

            // Act
            controller.ActivateOlItem(mail.Object);

            // Assert
            explorer.Verify(e => e.ClearSelection(), Times.Once);
            explorer.Verify(e => e.AddToSelection(mail.Object), Times.Once);
            mail.Verify(m => m.Display(It.IsAny<object>()), Times.Never);
        }

        [TestMethod]
        public void ActivateOlItem_WhenNotSelectable_DisplaysMailItem()
        {
            // Arrange
            var mail = new Mock<MailItem>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(false);
            var controller = MakeController(GlobalsWithExplorer(explorer));

            // Act
            controller.ActivateOlItem(mail.Object);

            // Assert
            mail.Verify(m => m.Display(It.IsAny<object>()), Times.Once);
            explorer.Verify(e => e.AddToSelection(It.IsAny<object>()), Times.Never);
        }

        [TestMethod]
        public void ActivateOlItem_WhenNotSelectable_DisplaysTaskItem()
        {
            // Arrange
            var task = new Mock<TaskItem>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(false);
            var controller = MakeController(GlobalsWithExplorer(explorer));

            // Act
            controller.ActivateOlItem(task.Object);

            // Assert
            task.Verify(t => t.Display(It.IsAny<object>()), Times.Once);
            explorer.Verify(e => e.AddToSelection(It.IsAny<object>()), Times.Never);
        }

        // ---------- ActivateOlItemAsync (async) ----------

        [TestMethod]
        public async Task ActivateOlItemAsync_WhenSelectable_ClearsAddsAndActivates()
        {
            // Arrange
            var mail = new Mock<MailItem>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(true);
            var controller = MakeController(GlobalsWithExplorer(explorer));

            // Act
            await controller.ActivateOlItemAsync(mail.Object);

            // Assert
            explorer.Verify(e => e.ClearSelection(), Times.Once);
            explorer.Verify(e => e.AddToSelection(mail.Object), Times.Once);
            explorer.Verify(e => e.Activate(), Times.Once);
        }

        [TestMethod]
        public async Task ActivateOlItemAsync_WhenNotSelectable_DisplaysAndActivates()
        {
            // Arrange
            var mail = new Mock<MailItem>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(false);
            var controller = MakeController(GlobalsWithExplorer(explorer));

            // Act
            await controller.ActivateOlItemAsync(mail.Object);

            // Assert
            mail.Verify(m => m.Display(It.IsAny<object>()), Times.Once);
            explorer.Verify(e => e.AddToSelection(It.IsAny<object>()), Times.Never);
            explorer.Verify(e => e.Activate(), Times.Once);
        }

        // ---------- TreeLvActivateItem valid-type caller path ----------

        [TestMethod]
        public void TreeLvActivateItem_WhenValidType_ActivatesSelectedItem()
        {
            // Arrange
            var mail = new Mock<MailItem>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(true);
            var globals = GlobalsWithExplorer(explorer);
            var viewer = new Mock<ITaskTreeForm>();
            viewer.Setup(v => v.GetSelectedNode()).Returns(NodeWithInner(mail.Object));
            var controller = new TaskTreeController(globals.Object, viewer.Object, EmptyModel());

            // Act
            controller.TreeLvActivateItem();

            // Assert
            explorer.Verify(e => e.AddToSelection(mail.Object), Times.Once);
        }

        [TestMethod]
        public async Task TreeLvActivateItemAsync_WhenValidType_ActivatesSelectedItem()
        {
            // Arrange
            var mail = new Mock<MailItem>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(true);
            var globals = GlobalsWithExplorer(explorer);
            var viewer = new Mock<ITaskTreeForm>();
            viewer.Setup(v => v.GetSelectedNode()).Returns(NodeWithInner(mail.Object));
            var controller = new TaskTreeController(globals.Object, viewer.Object, EmptyModel());

            // Act
            await controller.TreeLvActivateItemAsync();

            // Assert
            explorer.Verify(e => e.AddToSelection(mail.Object), Times.Once);
            explorer.Verify(e => e.Activate(), Times.Once);
        }
    }
}
