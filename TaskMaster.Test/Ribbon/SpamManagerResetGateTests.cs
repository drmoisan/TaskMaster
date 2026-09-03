using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for issue #735, finding 2. Before the fix,
    /// <c>RibbonController.ClearSpamManagerAsync</c> dereferenced the globals chain — the globals
    /// object, its auto-file objects, that container's classifier manager, and the engines facade —
    /// with no guard. Every link is genuinely null between ribbon construction and the completion
    /// of add-in initialization, so confirming the Clear Spam Manager prompt in that window threw an
    /// unhandled <see cref="NullReferenceException"/> out of a user-interface event handler.
    /// </summary>
    /// <remarks>
    /// The decision the defect turned on is extracted into <c>SpamManagerResetGate</c>, which is
    /// host-neutral and carries no coverage exemption. These tests cover every branch of it. No test
    /// here sleeps, polls, reads the wall clock, touches the filesystem, creates a temporary file,
    /// or starts a message pump. The one concrete type on the gate's boundary,
    /// <see cref="ManagerAsyncLazy"/>, is constructed over a mocked globals object: its constructor
    /// performs a field assignment and an async-lazy assignment that does not execute its factory,
    /// so construction reaches no disk and no COM.
    /// </remarks>
    [TestClass]
    public class SpamManagerResetGateTests
    {
        /// <summary>
        /// An accessor that fails the test if it is ever invoked. Used by the null-reset case to
        /// prove the argument check happens before any accessor is probed.
        /// </summary>
        private static Func<T> StrictAccessor<T>(string role)
            where T : class
        {
            return () =>
                throw new InvalidOperationException(
                    $"the {role} accessor must not be invoked when the reset delegate is null"
                );
        }

        /// <summary>
        /// Builds a real <see cref="ManagerAsyncLazy"/> over a mocked globals object.
        /// </summary>
        private static ManagerAsyncLazy CreateManager()
        {
            return new ManagerAsyncLazy(new Mock<IApplicationGlobals>().Object);
        }

        /// <summary>
        /// Builds an auto-file-objects accessor whose container reports the supplied manager.
        /// </summary>
        private static Func<IAppAutoFileObjects> CreateAutoFileAccessor(ManagerAsyncLazy manager)
        {
            var autoFile = new Mock<IAppAutoFileObjects>();
            autoFile.SetupGet(x => x.Manager).Returns(manager);
            return () => autoFile.Object;
        }

        #region Constructor argument validation

        [TestMethod]
        public void Constructor_WithNullAutoFileAccessor_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () =>
                new SpamManagerResetGate(null!, () => new Mock<IAppItemEngines>().Object, _ => { });

            // Assert
            act.Should()
                .Throw<ArgumentNullException>(
                    "the gate cannot resolve the classifier manager without an auto-file accessor"
                )
                .WithParameterName("autoFileAccessor");
        }

        [TestMethod]
        public void Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () =>
                new SpamManagerResetGate(
                    () => new Mock<IAppAutoFileObjects>().Object,
                    null!,
                    _ => { }
                );

            // Assert
            act.Should()
                .Throw<ArgumentNullException>(
                    "the gate cannot resolve the engines facade without an engines accessor"
                )
                .WithParameterName("enginesAccessor");
        }

        [TestMethod]
        public void Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () =>
                new SpamManagerResetGate(
                    () => new Mock<IAppAutoFileObjects>().Object,
                    () => new Mock<IAppItemEngines>().Object,
                    null!
                );

            // Assert
            act.Should()
                .Throw<ArgumentNullException>(
                    "a blocked invocation must have somewhere to report the not-ready notice"
                )
                .WithParameterName("notifyNotReady");
        }

        #endregion Constructor argument validation

        #region RunAsync argument validation

        /// <summary>
        /// A null reset delegate is a caller defect, not a "not ready" condition, so it must surface
        /// as an exception rather than being masked by a notice. The accessors are strict: if the
        /// argument check were ordered after them, they would throw and the assertion below would
        /// observe the wrong exception type.
        /// </summary>
        [TestMethod]
        public void RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors()
        {
            // Arrange
            var notifications = new List<string>();
            var gate = new SpamManagerResetGate(
                StrictAccessor<IAppAutoFileObjects>("auto-file"),
                StrictAccessor<IAppItemEngines>("engines"),
                notifications.Add
            );

            // Act
            Action act = () => gate.RunAsync(null!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>(
                    "the null-argument check must run before either accessor is probed"
                )
                .WithParameterName("reset");
            notifications.Should().BeEmpty("a caller defect is not a not-ready condition");
        }

        #endregion RunAsync argument validation

        #region Not-ready branches

        /// <summary>
        /// The pre-initialization window before the ribbon controller has been given its globals:
        /// the auto-file accessor itself returns null.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_WhenAutoFileAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset()
        {
            // Arrange
            var notifications = new List<string>();
            var resetInvocations = 0;
            var gate = new SpamManagerResetGate(
                () => null!,
                () => new Mock<IAppItemEngines>().Object,
                notifications.Add
            );

            // Act
            await gate.RunAsync(
                (manager, engines) =>
                {
                    resetInvocations++;
                    return Task.CompletedTask;
                }
            );

            // Assert
            resetInvocations
                .Should()
                .Be(0, "the reset must not run while the auto-file container is absent");
            notifications.Should().ContainSingle("a blocked invocation notifies exactly once");
        }

        /// <summary>
        /// The container exists but its classifier manager has not been assigned yet. The manager is
        /// an auto-property with no initializer, populated only inside the load paths, so Moq's
        /// default null for an unset property models the real state faithfully.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_WhenManagerIsNull_NotifiesOnceAndDoesNotInvokeReset()
        {
            // Arrange
            var notifications = new List<string>();
            var resetInvocations = 0;
            var autoFile = new Mock<IAppAutoFileObjects>();
            var gate = new SpamManagerResetGate(
                () => autoFile.Object,
                () => new Mock<IAppItemEngines>().Object,
                notifications.Add
            );

            // Act
            await gate.RunAsync(
                (manager, engines) =>
                {
                    resetInvocations++;
                    return Task.CompletedTask;
                }
            );

            // Assert
            resetInvocations
                .Should()
                .Be(0, "the reset must not run while the classifier manager is unset");
            notifications.Should().ContainSingle("a blocked invocation notifies exactly once");
        }

        /// <summary>
        /// The manager is present but the engines facade has not been populated. Both links are
        /// independently null in the same window, so each needs its own branch.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_WhenEnginesAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset()
        {
            // Arrange
            var notifications = new List<string>();
            var resetInvocations = 0;
            var gate = new SpamManagerResetGate(
                CreateAutoFileAccessor(CreateManager()),
                () => null!,
                notifications.Add
            );

            // Act
            await gate.RunAsync(
                (manager, engines) =>
                {
                    resetInvocations++;
                    return Task.CompletedTask;
                }
            );

            // Assert
            resetInvocations
                .Should()
                .Be(0, "the reset must not run while the engines facade is absent");
            notifications.Should().ContainSingle("a blocked invocation notifies exactly once");
        }

        #endregion Not-ready branches

        #region Open-gate behavior

        /// <summary>
        /// When both dependencies resolve, the gate passes them through by identity so the deferred
        /// work never has to re-read the globals chain it was extracted from.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_WhenAllDependenciesAvailable_InvokesResetWithResolvedManagerAndEngines()
        {
            // Arrange
            var notifications = new List<string>();
            var manager = CreateManager();
            var engines = new Mock<IAppItemEngines>().Object;
            var gate = new SpamManagerResetGate(
                CreateAutoFileAccessor(manager),
                () => engines,
                notifications.Add
            );
            ManagerAsyncLazy observedManager = null!;
            IAppItemEngines observedEngines = null!;

            // Act
            await gate.RunAsync(
                (resolvedManager, resolvedEngines) =>
                {
                    observedManager = resolvedManager;
                    observedEngines = resolvedEngines;
                    return Task.CompletedTask;
                }
            );

            // Assert
            observedManager
                .Should()
                .BeSameAs(manager, "the resolved manager is passed through by identity");
            observedEngines
                .Should()
                .BeSameAs(engines, "the resolved engines facade is passed through by identity");
            notifications.Should().BeEmpty("an open gate emits no not-ready notice");
        }

        /// <summary>
        /// The gate suppresses invocation, never errors. It returns the reset task directly with no
        /// await and no catch, so a fault from the deferred work propagates unchanged rather than
        /// being converted into a notice.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify()
        {
            // Arrange
            var notifications = new List<string>();
            var failure = new InvalidOperationException("the deferred reset failed");
            var gate = new SpamManagerResetGate(
                CreateAutoFileAccessor(CreateManager()),
                () => new Mock<IAppItemEngines>().Object,
                notifications.Add
            );

            // Act
            Func<Task> act = () => gate.RunAsync((manager, engines) => Task.FromException(failure));

            // Assert
            var thrown = await act.Should()
                .ThrowAsync<InvalidOperationException>(
                    "the gate contains no catch clause, so a fault from the deferred work escapes"
                );
            thrown.Which.Should().BeSameAs(failure, "the exception instance is not re-wrapped");
            notifications.Should().BeEmpty("a fault on the open path is not a not-ready condition");
        }

        #endregion Open-gate behavior
    }
}
