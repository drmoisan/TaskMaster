using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class QfcHomeControllerIterationTests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockApplicationGlobals;
        private Mock<System.Action> _mockParentCleanup;
        private QfcHomeController _controller;
        private Mock<Outlook.Application> _mockOlApp;
        private Mock<Explorer> _mockExplorer;

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            this._mockRepository = new MockRepository(MockBehavior.Strict);
            this._mockApplicationGlobals = this._mockRepository.Create<IApplicationGlobals>();
            this._mockApplicationGlobals.SetupGet(x => x.AF.CancelToken)
                .Returns(CancellationToken.None);

            this._mockOlApp = this._mockRepository.Create<Outlook.Application>();
            this._mockExplorer = this._mockRepository.Create<Explorer>();
            this._mockOlApp.Setup(x => x.ActiveExplorer()).Returns(_mockExplorer.Object);
            this._mockApplicationGlobals.SetupGet(x => x.Ol.App).Returns(_mockOlApp.Object);

            _ = SetUpMockIntelRes(_mockApplicationGlobals);

            _mockParentCleanup = new Mock<System.Action>();
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
        }

        private Mock<IntelligenceConfig> SetUpMockIntelRes(Mock<IApplicationGlobals> mockGlobals)
        {
            var intel = this._mockRepository.Create<IntelligenceConfig>(mockGlobals.Object);
            var config = new Dictionary<string, SmartSerializableLoader>
            {
                { "Folder", new SmartSerializableLoader() },
            }.ToConcurrentDictionary();
            intel.SetupGet(x => x.Config).Returns(config);
            mockGlobals.SetupGet(x => x.IntelRes).Returns(intel.Object);

            return intel;
        }

        [TestMethod]
        public async Task IterateQueueAsync_DataModelComplete()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(true);
            mockDataModel
                .Setup(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult((IList<MailItem>)new List<MailItem>()));
            var mockQfcQueue = new Mock<IQfcQueue>();
            mockQfcQueue
                .Setup(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            mockQfcQueue
                .Setup(m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        It.IsAny<IQfcCollectionController>()
                    )
                )
                .Returns(Task.CompletedTask);
            _controller.DataModel = mockDataModel.Object;
            _controller.QfcQueue = mockQfcQueue.Object;

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(
                m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never
            );
            mockQfcQueue.Verify(
                m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()),
                Times.Never
            );
            mockQfcQueue.Verify(
                m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        It.IsAny<IQfcCollectionController>()
                    ),
                Times.Never
            );
        }

        [TestMethod]
        public async Task IterateQueueAsync_QueueEmpty()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(false);
            mockDataModel
                .Setup(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult((IList<MailItem>)new List<MailItem>()));
            _controller.DataModel = mockDataModel.Object;

            var mockQfcQueue = new Mock<IQfcQueue>();
            mockQfcQueue
                .Setup(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            mockQfcQueue
                .Setup(m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        It.IsAny<IQfcCollectionController>()
                    )
                )
                .Returns(Task.CompletedTask);
            _controller.QfcQueue = mockQfcQueue.Object;

            // Mock the QfcFormController
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.Setup(m => m.ItemsPerIteration).Returns(8);
            var mockQfcCollectionController = new Mock<IQfcCollectionController>();
            mockFormController.Setup(m => m.Groups).Returns(mockQfcCollectionController.Object);
            _controller
                .GetType()
                .GetField(
                    "_formController",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormController.Object);

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(
                m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Once
            );
            mockQfcQueue.Verify(
                m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()),
                Times.Once
            );
            mockQfcQueue.Verify(
                m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        It.IsAny<IQfcCollectionController>()
                    ),
                Times.Never
            );
        }

        [TestMethod]
        public async Task IterateQueueAsync_Queue2()
        {
            // Arrange

            // Mock DataModel
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(false);

            // Setup DequeueNextItemGroupAsync to return 2 mail items
            var mockMailItem = new Mock<MailItem>();
            IList<MailItem> mailItems = new List<MailItem>
            {
                mockMailItem.Object,
                mockMailItem.Object,
            };
            mockDataModel
                .Setup(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(mailItems));

            // Set the DataModel in the controller to the mock
            _controller.DataModel = mockDataModel.Object;

            // Mock the QfcQueue
            var mockQfcQueue = new Mock<IQfcQueue>();
            mockQfcQueue
                .Setup(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            mockQfcQueue
                .Setup(m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        It.IsAny<IQfcCollectionController>()
                    )
                )
                .Returns(Task.CompletedTask);
            _controller.QfcQueue = mockQfcQueue.Object;

            // Mock the QfcFormController
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.Setup(m => m.ItemsPerIteration).Returns(8);
            var mockQfcCollectionController = new Mock<IQfcCollectionController>();
            mockFormController.Setup(m => m.Groups).Returns(mockQfcCollectionController.Object);
            _controller
                .GetType()
                .GetField(
                    "_formController",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormController.Object);

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(
                m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Once
            );
            mockQfcQueue.Verify(
                m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()),
                Times.Never
            );
            mockQfcQueue.Verify(
                m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        It.IsAny<IQfcCollectionController>()
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public async Task IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems()
        {
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(false);
            var mailItems = Enumerable
                .Range(0, 8)
                .Select(_ => new Mock<MailItem>().Object)
                .ToList();
            mockDataModel
                .Setup(m => m.DequeueNextItemGroupAsync(8, 2000))
                .Returns(Task.FromResult((IList<MailItem>)mailItems));
            _controller.DataModel = mockDataModel.Object;

            var mockQfcQueue = new Mock<IQfcQueue>();
            mockQfcQueue
                .Setup(m =>
                    m.EnqueueAsync(
                        It.Is<IList<MailItem>>(items => items.SequenceEqual(mailItems)),
                        It.IsAny<IQfcCollectionController>()
                    )
                )
                .Returns(Task.CompletedTask);
            _controller.QfcQueue = mockQfcQueue.Object;

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.Setup(m => m.ItemsPerIteration).Returns(8);
            var mockQfcCollectionController = new Mock<IQfcCollectionController>();
            mockFormController.Setup(m => m.Groups).Returns(mockQfcCollectionController.Object);
            _controller
                .GetType()
                .GetField(
                    "_formController",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormController.Object);

            await _controller.IterateQueueAsync();

            mockQfcQueue.Verify(
                m =>
                    m.EnqueueAsync(
                        It.Is<IList<MailItem>>(items => items.SequenceEqual(mailItems)),
                        mockQfcCollectionController.Object
                    ),
                Times.Once
            );
            mockQfcQueue.Verify(
                m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()),
                Times.Never
            );
        }

        [TestMethod]
        public void Iterate_ExecutesCorrectly()
        {
            // Arrange

            // Setup the DataModel to return 2 mocked mail items
            var mockDataModel = new Mock<IQfcDatamodel>();
            var mockMailItem = new Mock<MailItem>();
            IList<MailItem> mailItems = new List<MailItem>
            {
                mockMailItem.Object,
                mockMailItem.Object,
            };
            mockDataModel.Setup(m => m.DequeueNextItemGroup(It.IsAny<int>())).Returns(mailItems);
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            _controller
                .GetType()
                .GetField(
                    "_formController",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormController.Object);

            // Act
            _controller.Iterate();

            // Assert
            mockDataModel.Verify(m => m.DequeueNextItemGroup(It.IsAny<int>()), Times.Once);

            mockFormController.Verify(
                m =>
                    m.LoadItems(
                        It.Is<IList<MailItem>>(items =>
                            items.Count == 2 && items.Contains(mockMailItem.Object)
                        )
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public void Iterate2_ExecutesCorrectly()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(true);
            var mockQfcQueue = new Mock<IQfcQueue>();
            var mockFormController = new Mock<IQfcFormController>();
            _controller.QfcQueue = mockQfcQueue.Object;
            _controller
                .GetType()
                .GetField(
                    "_formController",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormController.Object);
            _controller.DataModel = mockDataModel.Object;

            // Act
            _controller.Iterate2();

            // Assert
            mockQfcQueue.Verify(m => m.Dequeue(), Times.Once);
            mockFormController.Verify(
                m => m.LoadItems(It.IsAny<TableLayoutPanel>(), It.IsAny<List<QfcItemGroup>>()),
                Times.Once
            );
        }

        [TestMethod]
        public void SwapStopWatch_ExecutesCorrectly()
        {
            // Arrange
            var stopWatch = new Stopwatch();
            _controller
                .GetType()
                .GetField(
                    "_stopWatch",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, stopWatch);

            // Act
            _controller.SwapStopWatch();

            // Assert
            var actual =
                _controller
                    .GetType()
                    .GetField(
                        "_stopWatchMoved",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    )
                    .GetValue(_controller) as Stopwatch;
            Assert.AreEqual(stopWatch, actual);
        }
    }
}
