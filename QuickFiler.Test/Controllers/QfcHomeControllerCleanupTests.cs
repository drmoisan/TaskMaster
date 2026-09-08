using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #791 AC2 coverage for <c>QfcHomeController.Cleanup()</c>: the ribbon release callback
    /// must run under a <c>finally</c>, the cancellation token source must be disposed, and the
    /// worker-completed handler must be detached before the viewer reference is dropped.
    /// <para>
    /// The controller is built through the public
    /// <c>QfcHomeController(IApplicationGlobals, System.Action)</c> constructor and its private
    /// fields are injected by reflection, exactly as <c>QfcHomeControllerPropertyTests</c> already
    /// does. No window is shown and no Outlook COM is touched.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcHomeControllerCleanupTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private static void SetPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, PrivateInstance);
            field
                .Should()
                .NotBeNull($"private field '{name}' should exist on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        /// <summary>
        /// AC2: if the datamodel cleanup throws, <c>RibbonController.ReleaseQuickFiler</c> must still
        /// run. Without the <c>finally</c> it never runs and both ribbon buttons become no-ops for
        /// the rest of the Outlook session, which is unrecoverable without restarting Outlook.
        /// </summary>
        [TestMethod]
        public void Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup()
        {
            // Arrange
            var parentCleanup = new Mock<System.Action>();
            var dataModel = new Mock<IQfcDatamodel>();
            dataModel
                .Setup(x => x.Cleanup())
                .Throws(new InvalidOperationException("datamodel cleanup failed"));
            var controller = new QfcHomeController(
                new Mock<IApplicationGlobals>().Object,
                parentCleanup.Object
            );
            SetPrivateField(controller, "_datamodel", dataModel.Object);

            // Act
            Action act = () => controller.Cleanup();

            // Assert
            act.Should().NotThrow("a failing cleanup stage must be logged, not propagated");
            parentCleanup.Verify(
                x => x.Invoke(),
                Times.Once,
                "the release callback runs under finally, whichever stage threw"
            );
        }

        /// <summary>
        /// AC2: the token source is disposed and the worker-completed handler is detached. An
        /// undisposed <see cref="CancellationTokenSource"/> leaks its registrations, and a
        /// worker-completed handler still attached after teardown runs against a nulled viewer.
        /// Disposal is observed by reading <see cref="CancellationTokenSource.Token"/> afterwards,
        /// which is the documented post-dispose throw; the detach is observed by the viewer mock's
        /// <c>Worker</c> getter having been read, which only the detach path does during cleanup.
        /// </summary>
        [TestMethod]
        public void Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted()
        {
            // Arrange
            var parentCleanup = new Mock<System.Action>();
            var formViewer = new Mock<IQfcFormViewer>();
            using (var worker = new BackgroundWorker())
            {
                formViewer.SetupGet(x => x.Worker).Returns(worker);
                var tokenSource = new CancellationTokenSource();
                var datamodel = new Mock<IQfcDatamodel>();
                var controller = new QfcHomeController(
                    new Mock<IApplicationGlobals>().Object,
                    parentCleanup.Object
                );
                SetPrivateField(controller, "_datamodel", datamodel.Object);
                SetPrivateField(controller, "_formViewer", formViewer.Object);
                SetPrivateField(controller, "_tokenSource", tokenSource);

                // Act
                controller.Cleanup();

                // Assert
                Action readToken = () =>
                {
                    CancellationToken _ = tokenSource.Token;
                };
                readToken
                    .Should()
                    .Throw<ObjectDisposedException>(
                        "the token source must be disposed during cleanup"
                    );
                formViewer.VerifyGet(
                    x => x.Worker,
                    Times.AtLeastOnce,
                    "the worker-completed handler must be detached before the viewer is dropped"
                );
                parentCleanup.Verify(x => x.Invoke(), Times.Once);

                // Issue #810 (AC3): a second cleanup must not re-run the datamodel stage. The
                // fields the first pass consumed are nulled, so the second pass has nothing left
                // to clean and the datamodel sees exactly one call across both passes.
                controller.Cleanup();
                datamodel.Verify(
                    x => x.Cleanup(),
                    Times.Once,
                    "cleanup must null the datamodel so a repeat pass cannot clean it twice"
                );
            }
        }

        /// <summary>
        /// Issue #810 (AC3): <c>Cleanup()</c> disposes the cancellation token source but must also
        /// null the field. A disposed source left reachable through
        /// <see cref="QfcHomeController.TokenSource"/> lets a later cancel call
        /// <c>Cancel()</c> on it and take an <see cref="ObjectDisposedException"/> on a teardown
        /// path that has no handler for one.
        /// </summary>
        [TestMethod]
        public void Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource()
        {
            // Arrange
            var parentCleanup = new Mock<System.Action>();
            var controller = new QfcHomeController(
                new Mock<IApplicationGlobals>().Object,
                parentCleanup.Object
            );
            SetPrivateField(controller, "_tokenSource", new CancellationTokenSource());
            controller
                .TokenSource.Should()
                .NotBeNull("the injected source must be reachable before cleanup runs");

            // Act
            controller.Cleanup();

            // Assert
            controller
                .TokenSource.Should()
                .BeNull("cleanup must null the field so no caller can reach the disposed source");
        }
    }
}
