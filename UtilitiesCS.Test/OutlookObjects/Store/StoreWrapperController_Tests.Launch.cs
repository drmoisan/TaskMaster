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
        /// rather than opening a broken dialog (AC1). Issue #287 adds a check that the shown
        /// copy is the state-specific <c>ModelUnavailable</c> title and message, not the
        /// former single unconditional literal (AC8).
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
            string capturedTitle = null;
            string capturedMessage = null;

            try
            {
                MyBox.DialogInvoker = viewer =>
                {
                    invocationCount++;
                    capturedTitle = viewer.Text;
                    capturedMessage = viewer.TextMessage.Text;
                    return DialogResult.OK;
                };

                // Act
                Action act = () => controller.Launch();

                // Assert
                act.Should().NotThrow();
                invocationCount.Should().Be(1);
                controller.Viewer.Should().BeNull();
                capturedTitle.Should().Be("Store Settings Unavailable");
                capturedMessage
                    .Should()
                    .Be(
                        "Store settings are not available. Retry once startup has completed; if the message persists, the store settings failed to load and the application log records the cause."
                    );
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
        /// leave <c>Viewer</c> null instead of opening a broken dialog (AC2). Issue #287 adds
        /// a check that the shown copy is the state-specific <c>StoresUnavailable</c> title
        /// and message, distinct from the <c>ModelUnavailable</c> copy (AC8).
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
            string capturedTitle = null;
            string capturedMessage = null;

            try
            {
                MyBox.DialogInvoker = viewer =>
                {
                    invocationCount++;
                    capturedTitle = viewer.Text;
                    capturedMessage = viewer.TextMessage.Text;
                    return DialogResult.OK;
                };

                // Act
                Action act = () => controller.Launch();

                // Assert
                act.Should().NotThrow();
                invocationCount.Should().Be(1);
                controller.Viewer.Should().BeNull();
                capturedTitle.Should().Be("Store Settings Loading");
                capturedMessage
                    .Should()
                    .Be("The store list has not finished loading. Please try again shortly.");
            }
            finally
            {
                MyBox.DialogInvoker = originalInvoker;
            }
        }

        /// <summary>
        /// Drives <c>Launch()</c> once per non-ready state through a single capturing invoker
        /// and asserts the two captured messages differ, proving the wiring surfaces
        /// state-specific copy end to end rather than one shared literal (AC8).
        /// </summary>
        [TestMethod]
        public void Launch_ForModelUnavailableAndStoresUnavailable_ShowsDifferentMessages()
        {
            // Arrange
            var mockGlobalsModelUnavailable = new Mock<IApplicationGlobals>();
            var mockOlModelUnavailable = new Mock<IOlObjects>();
            mockOlModelUnavailable.SetupGet(o => o.StoresWrapper).Returns((StoresWrapper)null);
            mockGlobalsModelUnavailable.SetupGet(g => g.Ol).Returns(mockOlModelUnavailable.Object);
            var controllerModelUnavailable = new StoreWrapperController(
                mockGlobalsModelUnavailable.Object
            );

            var mockGlobalsStoresUnavailable = new Mock<IApplicationGlobals>();
            var mockOlStoresUnavailable = new Mock<IOlObjects>();
            mockOlStoresUnavailable
                .SetupGet(o => o.StoresWrapper)
                .Returns(new StoresWrapper { Stores = null });
            mockGlobalsStoresUnavailable
                .SetupGet(g => g.Ol)
                .Returns(mockOlStoresUnavailable.Object);
            var controllerStoresUnavailable = new StoreWrapperController(
                mockGlobalsStoresUnavailable.Object
            );

            var originalInvoker = MyBox.DialogInvoker;
            string capturedModelUnavailableMessage = null;
            string capturedStoresUnavailableMessage = null;

            try
            {
                MyBox.DialogInvoker = viewer =>
                {
                    if (capturedModelUnavailableMessage is null)
                    {
                        capturedModelUnavailableMessage = viewer.TextMessage.Text;
                    }
                    else
                    {
                        capturedStoresUnavailableMessage = viewer.TextMessage.Text;
                    }
                    return DialogResult.OK;
                };

                // Act
                controllerModelUnavailable.Launch();
                controllerStoresUnavailable.Launch();

                // Assert
                capturedModelUnavailableMessage.Should().NotBe(capturedStoresUnavailableMessage);
            }
            finally
            {
                MyBox.DialogInvoker = originalInvoker;
            }
        }

        #endregion

        #region BuildUnavailableMessage and BuildUnavailableTitle (issue #287)

        /// <summary>
        /// For <c>ModelUnavailable</c>, <c>BuildUnavailableMessage</c> returns the bounded
        /// retry copy naming the application log, not the pre-#287 unconditional phrase.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableMessage_WhenModelUnavailable_ReturnsBoundedRetryCopy()
        {
            // Arrange & Act
            var message = StoreLaunchReadinessEvaluator.BuildUnavailableMessage(
                StoreLaunchReadinessState.ModelUnavailable
            );

            // Assert
            message
                .Should()
                .Be(
                    "Store settings are not available. Retry once startup has completed; if the message persists, the store settings failed to load and the application log records the cause."
                );
        }

        /// <summary>
        /// For <c>StoresUnavailable</c>, <c>BuildUnavailableMessage</c> returns the
        /// still-loading copy, distinct from the genuine-failure copy.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableMessage_WhenStoresUnavailable_ReturnsStillLoadingCopy()
        {
            // Arrange & Act
            var message = StoreLaunchReadinessEvaluator.BuildUnavailableMessage(
                StoreLaunchReadinessState.StoresUnavailable
            );

            // Assert
            message
                .Should()
                .Be("The store list has not finished loading. Please try again shortly.");
        }

        /// <summary>
        /// <c>BuildUnavailableMessage</c> has no message for a ready model and must throw
        /// rather than return a misleading string.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableMessage_WhenReady_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Action act = () =>
                StoreLaunchReadinessEvaluator.BuildUnavailableMessage(
                    StoreLaunchReadinessState.Ready
                );

            // Act & Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        /// <summary>
        /// An undefined enum value produced by a cast falls into the discard arm, which
        /// carries the same conservative <c>ModelUnavailable</c> copy.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableMessage_WhenStateIsUndefinedCast_ReturnsModelUnavailableCopy()
        {
            // Arrange & Act
            var message = StoreLaunchReadinessEvaluator.BuildUnavailableMessage(
                (StoreLaunchReadinessState)99
            );

            // Assert
            message
                .Should()
                .Be(
                    "Store settings are not available. Retry once startup has completed; if the message persists, the store settings failed to load and the application log records the cause."
                );
        }

        /// <summary>
        /// The two non-ready states must produce different message text so the user can tell
        /// a genuine failure apart from a still-loading state.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableMessage_ForTheTwoNonReadyStates_ReturnsDifferentStrings()
        {
            // Arrange & Act
            var modelUnavailableMessage = StoreLaunchReadinessEvaluator.BuildUnavailableMessage(
                StoreLaunchReadinessState.ModelUnavailable
            );
            var storesUnavailableMessage = StoreLaunchReadinessEvaluator.BuildUnavailableMessage(
                StoreLaunchReadinessState.StoresUnavailable
            );

            // Assert
            modelUnavailableMessage.Should().NotBe(storesUnavailableMessage);
        }

        /// <summary>
        /// For <c>ModelUnavailable</c>, <c>BuildUnavailableTitle</c> returns the genuine-failure
        /// title.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableTitle_WhenModelUnavailable_ReturnsStoreSettingsUnavailableTitle()
        {
            // Arrange & Act
            var title = StoreLaunchReadinessEvaluator.BuildUnavailableTitle(
                StoreLaunchReadinessState.ModelUnavailable
            );

            // Assert
            title.Should().Be("Store Settings Unavailable");
        }

        /// <summary>
        /// For <c>StoresUnavailable</c>, <c>BuildUnavailableTitle</c> returns the
        /// still-loading title, distinct from the genuine-failure title.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableTitle_WhenStoresUnavailable_ReturnsStoreSettingsLoadingTitle()
        {
            // Arrange & Act
            var title = StoreLaunchReadinessEvaluator.BuildUnavailableTitle(
                StoreLaunchReadinessState.StoresUnavailable
            );

            // Assert
            title.Should().Be("Store Settings Loading");
        }

        /// <summary>
        /// <c>BuildUnavailableTitle</c> has no title for a ready model and must throw rather
        /// than return a misleading string.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableTitle_WhenReady_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Action act = () =>
                StoreLaunchReadinessEvaluator.BuildUnavailableTitle(
                    StoreLaunchReadinessState.Ready
                );

            // Act & Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        /// <summary>
        /// An undefined enum value produced by a cast falls into the discard arm, which
        /// carries the same conservative <c>ModelUnavailable</c> title.
        /// </summary>
        [TestMethod]
        public void BuildUnavailableTitle_WhenStateIsUndefinedCast_ReturnsModelUnavailableTitle()
        {
            // Arrange & Act
            var title = StoreLaunchReadinessEvaluator.BuildUnavailableTitle(
                (StoreLaunchReadinessState)99
            );

            // Assert
            title.Should().Be("Store Settings Unavailable");
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
