using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class QfcQueueCoverageExpansionTests
    {
        private const BindingFlags NonPublicInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        private static QfcQueue NewQueue()
        {
            var globals = new Mock<IApplicationGlobals>().Object;
            return new QfcQueue(CancellationToken.None, (QfcHomeController)null, globals);
        }

        private static BlockingCollection<(
            TableLayoutPanel Tlp,
            List<QfcItemGroup> ItemGroups
        )> NewBlockingQueue(params (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)[] entries)
        {
            var queue =
                new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>();
            foreach (var entry in entries)
            {
                queue.Add(entry);
            }

            return queue;
        }

        private static MailItem NewMailItem(string entryId)
        {
            var mailItem = new Mock<MailItem>();
            mailItem.Setup(x => x.EntryID).Returns(entryId);
            return mailItem.Object;
        }

        private static Mock<IQfcItemController> NewItemController()
        {
            var controller = new Mock<IQfcItemController>();
            controller.SetupProperty(x => x.ItemNumber);
            controller.SetupProperty(x => x.ItemNumberDigits);
            return controller;
        }

        private static QfcItemGroup NewGroup(string entryId)
        {
            return NewGroup(NewMailItem(entryId), NewItemController().Object);
        }

        private static QfcItemGroup NewGroup(MailItem mailItem, IQfcItemController controller)
        {
            return new QfcItemGroup(mailItem) { ItemController = controller };
        }

        private static TableLayoutPanel NewTableLayoutPanel(int rows, int heightPerRow = 20)
        {
            var panel = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = rows,
                MinimumSize = new Size(100, rows * heightPerRow),
            };
            for (int i = 0; i < rows; i++)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, heightPerRow));
            }

            return panel;
        }

        private static Panel NewPanel(string name)
        {
            return new Panel { Name = name };
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull($"private field '{name}' should exist on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        [TestMethod]
        public void Dequeue_WithQueuedEntry_UnhooksItemsRaisesRemoveAndUpdatesCount()
        {
            var queue = NewQueue();
            var firstMailItem = NewMailItem("first");
            var secondMailItem = NewMailItem("second");
            var groups = new List<QfcItemGroup>
            {
                NewGroup(firstMailItem, NewItemController().Object),
                NewGroup(secondMailItem, NewItemController().Object),
            };
            var backingQueue = NewBlockingQueue((NewTableLayoutPanel(1), groups));
            var moveMonitor = new Mock<IEmailMoveMonitor>(MockBehavior.Strict);
            moveMonitor.Setup(x => x.UnhookItem(firstMailItem));
            moveMonitor.Setup(x => x.UnhookItem(secondMailItem));
            NotifyCollectionChangedEventArgs observedArgs = null;

            SetPrivateField(queue, "_queue", backingQueue);
            SetPrivateField(queue, "_moveMonitor", moveMonitor.Object);
            queue.CollectionChanged += (sender, args) => observedArgs = args;

            var result = queue.Dequeue();

            result.ItemGroups.Should().Equal(groups);
            queue.Count.Should().Be(0);
            observedArgs.Should().NotBeNull();
            observedArgs.Action.Should().Be(NotifyCollectionChangedAction.Remove);
            moveMonitor.Verify(x => x.UnhookItem(firstMailItem), Times.Once);
            moveMonitor.Verify(x => x.UnhookItem(secondMailItem), Times.Once);
        }

        [TestMethod]
        public async Task TryDequeueAsync_WithCompletedPendingEntry_UnhooksItemsAndRaisesRemove()
        {
            var queue = NewQueue();
            var mailItem = NewMailItem("pending");
            var groups = new List<QfcItemGroup> { NewGroup(mailItem, NewItemController().Object) };
            var backingQueue = NewBlockingQueue((NewTableLayoutPanel(1), groups));
            backingQueue.CompleteAdding();
            var moveMonitor = new Mock<IEmailMoveMonitor>(MockBehavior.Strict);
            moveMonitor.Setup(x => x.UnhookItem(mailItem));
            NotifyCollectionChangedEventArgs observedArgs = null;

            SetPrivateField(queue, "_queue", backingQueue);
            SetPrivateField(queue, "_moveMonitor", moveMonitor.Object);
            queue.CollectionChanged += (sender, args) => observedArgs = args;

            var result = await queue.TryDequeueAsync(CancellationToken.None, timeout: 50);

            result.ItemGroups.Should().Equal(groups);
            observedArgs.Should().NotBeNull();
            observedArgs.Action.Should().Be(NotifyCollectionChangedAction.Remove);
            moveMonitor.Verify(x => x.UnhookItem(mailItem), Times.Once);
        }

        [TestMethod]
        public async Task TryDequeueAsync_WithRunningJobAndCancellation_ReturnsDefault()
        {
            var queue = NewQueue();
            SetPrivateField(
                queue,
                "_queue",
                new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>()
            );
            SetPrivateField(queue, "_jobsRunning", 1);

            using (var tokenSource = new CancellationTokenSource())
            {
                tokenSource.CancelAfter(25);

                var result = await queue.TryDequeueAsync(tokenSource.Token, timeout: 250);

                result.Should().Be(default);
            }
        }

        [TestMethod]
        public async Task CompleteAddingAsync_WhenFunctionTimeoutExpires_ThrowsAndLeavesQueueOpen()
        {
            var queue = NewQueue();
            var backingQueue =
                new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>();
            SetPrivateField(queue, "_queue", backingQueue);
            SetPrivateField(queue, "_jobsRunning", 1);

            await queue
                .Awaiting(q => q.CompleteAddingAsync(CancellationToken.None, timeout: 25))
                .Should()
                .ThrowAsync<OperationCanceledException>();
            backingQueue.IsAddingCompleted.Should().BeFalse();
        }

        [TestMethod]
        public void Dequeue_WithHighConfidenceCarrier_PreservesPredeterminedFolder()
        {
            var queue = NewQueue();
            var mailItem = NewMailItem("high-confidence");
            var group = NewGroup(mailItem, NewItemController().Object);
            group.PredeterminedFolder = @"Archive\High Confidence";
            var backingQueue = NewBlockingQueue(
                (NewTableLayoutPanel(1), new List<QfcItemGroup> { group })
            );
            var moveMonitor = new Mock<IEmailMoveMonitor>(MockBehavior.Strict);
            moveMonitor.Setup(x => x.UnhookItem(mailItem));

            SetPrivateField(queue, "_queue", backingQueue);
            SetPrivateField(queue, "_moveMonitor", moveMonitor.Object);

            var result = queue.Dequeue();

            result.ItemGroups.Should().ContainSingle();
            result.ItemGroups[0].PredeterminedFolder.Should().Be(@"Archive\High Confidence");
            moveMonitor.Verify(x => x.UnhookItem(mailItem), Times.Once);
        }

        [TestMethod]
        public void AdjustTlp_WhenRowsIncrease_GrowsRowCountAndMinimumHeight()
        {
            var queue = NewQueue();
            var panel = NewTableLayoutPanel(rows: 1, heightPerRow: 20);
            var rowStyle = new RowStyle(SizeType.Absolute, 25);

            queue.AdjustTlp(panel, newRowCount: 3, rowStyle);

            panel.RowCount.Should().Be(4);
            panel.MinimumSize.Height.Should().Be(95);
        }

        [TestMethod]
        public void RenumberGroups_WithTenItems_UsesTwoDigitNumbersAndSequentialIndexes()
        {
            var queue = NewQueue();
            var controllers = new List<Mock<IQfcItemController>>();
            var groups = new List<QfcItemGroup>();
            for (int i = 0; i < 10; i++)
            {
                var controller = NewItemController();
                controllers.Add(controller);
                groups.Add(NewGroup(NewMailItem("mail-" + i), controller.Object));
            }

            queue.RenumberGroups(groups);

            for (int i = 0; i < controllers.Count; i++)
            {
                controllers[i].Object.ItemNumberDigits.Should().Be(2);
                controllers[i].Object.ItemNumber.Should().Be(i + 1);
            }
        }

        [TestMethod]
        public void GrowEntry_WhenTargetHasCapacity_MovesControlAndGroupThenResetsSourceState()
        {
            var queue = NewQueue();
            var targetController = NewItemController();
            var sourceController = NewItemController();
            var targetPanel = NewTableLayoutPanel(rows: 2, heightPerRow: 20);
            var sourcePanel = NewTableLayoutPanel(rows: 2, heightPerRow: 20);
            var targetControl = NewPanel("target");
            var sourceControl = NewPanel("source");
            targetPanel.Controls.Add(targetControl, 0, 0);
            sourcePanel.Controls.Add(sourceControl, 0, 0);
            var target = (
                Tlp: targetPanel,
                ItemGroups: new List<QfcItemGroup>
                {
                    NewGroup(NewMailItem("target"), targetController.Object),
                }
            );
            var source = (
                Tlp: sourcePanel,
                ItemGroups: new List<QfcItemGroup>
                {
                    NewGroup(NewMailItem("source"), sourceController.Object),
                }
            );
            var rowStyle = new RowStyle(SizeType.Absolute, 20);

            queue.GrowEntry(ref target, ref source, newRowCount: 2, rowStyle);

            target.ItemGroups.Should().HaveCount(2);
            source.ItemGroups.Should().BeEmpty();
            sourceControl.Parent.Should().BeSameAs(targetPanel);
            targetPanel.GetCellPosition(sourceControl).Row.Should().Be(1);
            sourcePanel.RowCount.Should().Be(1);
            sourcePanel.MinimumSize.Height.Should().Be(20);
            sourceController.Object.ItemNumber.Should().Be(2);
        }
    }
}
