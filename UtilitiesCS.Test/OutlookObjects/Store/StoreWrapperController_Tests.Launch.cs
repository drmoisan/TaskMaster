using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    public partial class StoreWrapperController_Tests
    {
        #region Launch (issue #240 regression)

        /// <summary>
        /// Reproduces issue #240: when the Outlook store-wrapper model has not yet been
        /// loaded (<c>Globals.Ol.StoresWrapper</c> is null), <c>Launch()</c> must not throw
        /// an unhandled <see cref="NullReferenceException"/>. It must show a user-facing
        /// message via the <see cref="MyBox"/> dialog seam and leave <c>Viewer</c> null
        /// rather than opening a broken dialog (AC1).
        /// </summary>
        [TestMethod]
        public void Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.SetupGet(o => o.StoresWrapper).Returns((StoresWrapper)null);
            mockGlobals.SetupGet(g => g.Ol).Returns(mockOl.Object);
            var controller = new StoreWrapperController(mockGlobals.Object);

            var originalInvoker = MyBox.DialogInvoker;
            var invocationCount = 0;

            try
            {
                MyBox.DialogInvoker = _ =>
                {
                    invocationCount++;
                    return DialogResult.OK;
                };

                // Act
                Action act = () => controller.Launch();

                // Assert
                act.Should().NotThrow();
                invocationCount.Should().Be(1);
                controller.Viewer.Should().BeNull();
            }
            finally
            {
                MyBox.DialogInvoker = originalInvoker;
            }
        }

        /// <summary>
        /// Reproduces issue #240 for the secondary root cause: a non-null
        /// <c>StoresWrapper</c> whose <c>Stores</c> list is transiently null (post-deserialize
        /// state before the async rewire completes). <c>Launch()</c> must not throw and must
        /// leave <c>Viewer</c> null instead of opening a broken dialog (AC2).
        /// </summary>
        [TestMethod]
        public void Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.SetupGet(o => o.StoresWrapper).Returns(new StoresWrapper { Stores = null });
            mockGlobals.SetupGet(g => g.Ol).Returns(mockOl.Object);
            var controller = new StoreWrapperController(mockGlobals.Object);

            var originalInvoker = MyBox.DialogInvoker;
            var invocationCount = 0;

            try
            {
                MyBox.DialogInvoker = _ =>
                {
                    invocationCount++;
                    return DialogResult.OK;
                };

                // Act
                Action act = () => controller.Launch();

                // Assert
                act.Should().NotThrow();
                invocationCount.Should().Be(1);
                controller.Viewer.Should().BeNull();
            }
            finally
            {
                MyBox.DialogInvoker = originalInvoker;
            }
        }

        #endregion

        #region EvaluateLaunchReadiness (issue #240)

        /// <summary>
        /// When <see cref="StoreWrapperController.Globals"/> is null, readiness cannot be
        /// determined and the evaluation must report <c>ModelUnavailable</c> rather than
        /// throwing.
        /// </summary>
        [TestMethod]
        public void EvaluateLaunchReadiness_WhenGlobalsIsNull_ReturnsModelUnavailable()
        {
            // Arrange
            var controller = new StoreWrapperController(null!);

            // Act
            var readiness = controller.EvaluateLaunchReadiness();

            // Assert
            readiness.State.Should().Be(StoreLaunchReadinessState.ModelUnavailable);
        }

        /// <summary>
        /// When <c>Globals.Ol</c> is null, readiness cannot be determined and the evaluation
        /// must report <c>ModelUnavailable</c> rather than throwing.
        /// </summary>
        [TestMethod]
        public void EvaluateLaunchReadiness_WhenOlIsNull_ReturnsModelUnavailable()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.SetupGet(g => g.Ol).Returns((IOlObjects)null);
            var controller = new StoreWrapperController(mockGlobals.Object);

            // Act
            var readiness = controller.EvaluateLaunchReadiness();

            // Assert
            readiness.State.Should().Be(StoreLaunchReadinessState.ModelUnavailable);
        }

        /// <summary>
        /// When <c>Globals.Ol.StoresWrapper</c> is null (store load has not completed),
        /// the evaluation must report <c>ModelUnavailable</c>.
        /// </summary>
        [TestMethod]
        public void EvaluateLaunchReadiness_WhenStoresWrapperIsNull_ReturnsModelUnavailable()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.SetupGet(o => o.StoresWrapper).Returns((StoresWrapper)null);
            mockGlobals.SetupGet(g => g.Ol).Returns(mockOl.Object);
            var controller = new StoreWrapperController(mockGlobals.Object);

            // Act
            var readiness = controller.EvaluateLaunchReadiness();

            // Assert
            readiness.State.Should().Be(StoreLaunchReadinessState.ModelUnavailable);
        }

        /// <summary>
        /// When the model is present but its <c>Stores</c> list is transiently null
        /// (post-deserialize, before the async rewire populates it), the evaluation must
        /// report <c>StoresUnavailable</c>.
        /// </summary>
        [TestMethod]
        public void EvaluateLaunchReadiness_WhenStoresListIsNull_ReturnsStoresUnavailable()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.SetupGet(o => o.StoresWrapper).Returns(new StoresWrapper { Stores = null });
            mockGlobals.SetupGet(g => g.Ol).Returns(mockOl.Object);
            var controller = new StoreWrapperController(mockGlobals.Object);

            // Act
            var readiness = controller.EvaluateLaunchReadiness();

            // Assert
            readiness.State.Should().Be(StoreLaunchReadinessState.StoresUnavailable);
        }

        /// <summary>
        /// When the model and its <c>Stores</c> list are both populated, the evaluation must
        /// report <c>Ready</c> with the model and the display names of every seeded store.
        /// </summary>
        [TestMethod]
        public void EvaluateLaunchReadiness_WhenModelAndStoresPopulated_ReturnsReadyWithDisplayNames()
        {
            // Arrange
            var storeA = new StoreWrapper(null) { DisplayName = "Mailbox A" };
            var storeB = new StoreWrapper(null) { DisplayName = "Mailbox B" };
            var model = new StoresWrapper
            {
                Stores = new List<StoreWrapper> { storeA, storeB },
            };
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.SetupGet(o => o.StoresWrapper).Returns(model);
            mockGlobals.SetupGet(g => g.Ol).Returns(mockOl.Object);
            var controller = new StoreWrapperController(mockGlobals.Object);

            // Act
            var readiness = controller.EvaluateLaunchReadiness();

            // Assert
            readiness.State.Should().Be(StoreLaunchReadinessState.Ready);
            readiness.DisplayNames.Should().Equal("Mailbox A", "Mailbox B");
        }

        #endregion

        #region Stub helpers

        private sealed class StubSelectFolderController : StoreWrapperController
        {
            private readonly FolderMinimalWrapper _stub;

            internal StubSelectFolderController(
                IApplicationGlobals globals,
                FolderMinimalWrapper stubFolder
            )
                : base(globals)
            {
                _stub = stubFolder;
            }

            internal override FolderMinimalWrapper SelectFolder() => _stub;
        }

        #endregion
    }
}
