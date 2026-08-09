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
    /// Unit tests for the issue #505/#506/#518 toggle-state coordinator: the synchronous
    /// <c>getPressed</c> cache, its lazy asynchronous prime, the update-before-invalidate ordering
    /// of the toggle path, and the observed-and-logged click boundary.
    /// </summary>
    /// <remarks>
    /// Every asynchronous outcome is driven by a <see cref="TaskCompletionSource{TResult}"/> and
    /// awaited through the coordinator's own prime handle, so no test sleeps, polls, reads the
    /// wall clock, touches the filesystem, or starts a message pump. No test drives a path that
    /// reaches <c>NotifyEngineCommandNotReady</c>: the notification sink is an injected delegate.
    /// </remarks>
    [TestClass]
    public class EngineToggleStateCoordinatorTests
    {
        private const string SpamEngine = "Spam";
        private const string SpamToggleControlId = "SpamBayesEnabledToggle";

        #region Constructor contracts

        [TestMethod]
        public void Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException()
        {
            // Act
            Action act = () =>
                new EngineToggleStateCoordinator(null, _ => { }, _ => { }, (_, _) => { });

            // Assert
            act.Should()
                .Throw<ArgumentNullException>("the accessor is required to reach the engines")
                .WithParameterName("enginesAccessor");
        }

        [TestMethod]
        public void Constructor_WithNullInvalidateDelegate_ThrowsArgumentNullException()
        {
            // Act
            Action act = () =>
                new EngineToggleStateCoordinator(
                    () => new Mock<IAppItemEngines>().Object,
                    null,
                    _ => { },
                    (_, _) => { }
                );

            // Assert
            act.Should()
                .Throw<ArgumentNullException>("without invalidation the checkbox never corrects")
                .WithParameterName("invalidateControl");
        }

        [TestMethod]
        public void Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException()
        {
            // Act
            Action act = () =>
                new EngineToggleStateCoordinator(
                    () => new Mock<IAppItemEngines>().Object,
                    _ => { },
                    null,
                    (_, _) => { }
                );

            // Assert
            act.Should()
                .Throw<ArgumentNullException>("a blocked click must always be able to notify")
                .WithParameterName("notifyUnavailable");
        }

        [TestMethod]
        public void Constructor_WithNullLogErrorDelegate_ThrowsArgumentNullException()
        {
            // Act
            Action act = () =>
                new EngineToggleStateCoordinator(
                    () => new Mock<IAppItemEngines>().Object,
                    _ => { },
                    _ => { },
                    null
                );

            // Assert
            act.Should()
                .Throw<ArgumentNullException>("an observed fault must always be reportable")
                .WithParameterName("logError");
        }

        #endregion Constructor contracts

        #region GetPressed — cached read semantics

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void GetPressed_WithNullOrWhitespaceKey_ReturnsFalseWithoutPrimeOrInvalidate(
            string engineName
        )
        {
            // Arrange
            var harness = new Harness();

            // Act
            var pressed = harness.Coordinator.GetPressed(engineName);

            // Assert
            pressed.Should().BeFalse("an unusable engine key must report the toggle as unchecked");
            harness.Engines.Verify(x => x.EngineActiveAsync(It.IsAny<string>()), Times.Never);
            harness.Invalidations.Should().BeEmpty("nothing changed, so nothing may be invalidated");
            harness
                .Coordinator.GetPrimeTask(engineName)
                .IsCompleted.Should()
                .BeTrue("no prime may be registered for an unusable key");
        }

        [TestMethod]
        public void GetPressed_WithUnmappedKey_ReturnsFalseWithoutPrime()
        {
            // Arrange
            var harness = new Harness();

            // Act
            var pressed = harness.Coordinator.GetPressed("NotAToggleBackedEngine");

            // Assert
            pressed.Should().BeFalse("an engine with no toggle checkbox reports unchecked");
            harness.Engines.Verify(x => x.EngineActiveAsync(It.IsAny<string>()), Times.Never);
            harness.Invalidations.Should().BeEmpty();
        }

        [TestMethod]
        public void GetPressed_WhenEnginesAccessorReturnsNull_ReturnsFalseAndStartsNothing()
        {
            // Arrange: the pre-SetGlobals window, where RibbonController.Engines yields null.
            var harness = new Harness { EnginesAvailable = false };

            // Act
            var pressed = harness.Coordinator.GetPressed(SpamEngine);

            // Assert
            pressed
                .Should()
                .BeFalse("an unknown state must degrade to unchecked rather than throwing");
            harness.Engines.Verify(x => x.EngineActiveAsync(It.IsAny<string>()), Times.Never);
            harness.Invalidations.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime()
        {
            // Arrange: the prime is held open, so the second read observes an in-flight prime.
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task);

            // Act
            var first = harness.Coordinator.GetPressed(SpamEngine);
            var second = harness.Coordinator.GetPressed(SpamEngine);

            // Assert
            first.Should().BeFalse("the state is not known until the prime completes");
            second.Should().BeFalse("an in-flight prime does not change the cached answer");
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Once,
                "a second read during an in-flight prime must not start a second prime"
            );
            harness.Invalidations.Should().BeEmpty("the prime has not completed yet");

            // Cleanup: complete the prime deterministically so no work is left in flight.
            probe.SetResult(false);
            await harness.Coordinator.GetPrimeTask(SpamEngine);
        }

        [TestMethod]
        public async Task GetPressed_AfterPrimeCompletes_ReturnsPrimedValueAndInvalidatesMappedControl()
        {
            // Arrange
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task);
            harness.Coordinator.GetPressed(SpamEngine);

            // Act
            probe.SetResult(true);
            await harness.Coordinator.GetPrimeTask(SpamEngine);

            // Assert
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeTrue("the completed prime is the new cached state");
            harness
                .Invalidations.Should()
                .Equal(
                    new[] { SpamToggleControlId },
                    "Office must be told to re-query getPressed for the mapped control"
                );
        }

        [TestMethod]
        public async Task GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse()
        {
            // Arrange
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            var failure = new InvalidOperationException("configuration load failed");
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task);
            harness.Coordinator.GetPressed(SpamEngine);

            // Act
            probe.SetException(failure);
            await harness.Coordinator.GetPrimeTask(SpamEngine);

            // Assert
            harness
                .Errors.Should()
                .ContainSingle("a prime fault must be observed exactly once, never left unobserved");
            harness.Errors[0].Message.Should().Contain(SpamEngine);
            harness.Errors[0].Exception.Should().BeSameAs(failure);
            harness.Invalidations.Should().BeEmpty("a failed prime changed no state to display");

            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeFalse("a failed prime leaves the toggle reporting unchecked");

            // Cleanup: the failed prime clears its marker, so the read above re-primed.
            await harness.Coordinator.GetPrimeTask(SpamEngine);
        }

        #endregion GetPressed — cached read semantics

        #region ExecuteToggleAsync — ordering and fault propagation

        [TestMethod]
        public async Task ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder()
        {
            // Arrange: every step appends to one sequence, including a cache probe performed from
            // inside the invalidation sink, which is where Office would re-query getPressed.
            var harness = new Harness();
            var sequence = new List<string>();
            harness
                .Engines.Setup(x => x.ToggleEngineAsync(SpamEngine))
                .Returns(() =>
                {
                    sequence.Add("ToggleEngineAsync");
                    return Task.CompletedTask;
                });
            harness
                .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                .Returns(() =>
                {
                    sequence.Add("EngineActiveAsync");
                    return Task.FromResult(true);
                });
            harness.OnInvalidate = controlId =>
            {
                sequence.Add(
                    "CacheVisible:" + harness.Coordinator.GetPressed(SpamEngine).ToString()
                );
                sequence.Add("Invalidate:" + controlId);
            };

            // Act
            await harness.Coordinator.ExecuteToggleAsync(SpamEngine);

            // Assert: update-before-invalidate is the invariant that prevents Office answering an
            // invalidation from stale state.
            sequence
                .Should()
                .Equal(
                    new[]
                    {
                        "ToggleEngineAsync",
                        "EngineActiveAsync",
                        "CacheVisible:True",
                        "Invalidate:" + SpamToggleControlId,
                    }
                );
        }

        [TestMethod]
        public async Task ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged()
        {
            // Arrange
            var harness = new Harness();
            var failure = new InvalidOperationException("toggle failed");
            harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).ThrowsAsync(failure);

            // Act
            Func<Task> act = () => harness.Coordinator.ExecuteToggleAsync(SpamEngine);

            // Assert: the testable core never catches, so the fault reaches the boundary intact.
            (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should()
                .BeSameAs(failure);
            harness.Invalidations.Should().BeEmpty("a failed toggle changed no state to display");
            harness.Errors.Should().BeEmpty("only the click boundary reports faults");
        }

        [TestMethod]
        public async Task ExecuteToggleAsync_WithUnmappedKey_ThrowsArgumentException()
        {
            // Arrange
            var harness = new Harness();

            // Act
            Func<Task> act = () =>
                harness.Coordinator.ExecuteToggleAsync("NotAToggleBackedEngine");

            // Assert: fail fast — an unmapped key has no control to invalidate.
            (await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("engineName");
            harness.Engines.Verify(x => x.ToggleEngineAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion ExecuteToggleAsync — ordering and fault propagation

        #region HandleToggleClickAsync — the observed boundary

        [TestMethod]
        public async Task HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate()
        {
            // Arrange
            var harness = new Harness();
            var failure = new InvalidOperationException("toggle failed");
            harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).ThrowsAsync(failure);

            // Act
            Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);

            // Assert
            await act.Should()
                .NotThrowAsync("an async void Office handler must never see an unobserved fault");
            harness.Errors.Should().ContainSingle();
            harness.Errors[0].Message.Should().Contain(SpamEngine);
            harness.Errors[0].Exception.Should().BeSameAs(failure);
            harness.Invalidations.Should().BeEmpty("a failed toggle changed no state to display");
            harness.Notifications.Should().BeEmpty("a fault is logged, not surfaced as a notice");
        }

        [TestMethod]
        public async Task HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing()
        {
            // Arrange: the pre-SetGlobals window.
            var harness = new Harness { EnginesAvailable = false };

            // Act
            Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);

            // Assert
            await act.Should().NotThrowAsync("a click before initialization must degrade quietly");
            harness
                .Notifications.Should()
                .ContainSingle("exactly one notice per blocked toggle click");
            harness.Notifications[0].Should().Contain(SpamEngine);
            harness.Engines.Verify(x => x.ToggleEngineAsync(It.IsAny<string>()), Times.Never);
            harness.Invalidations.Should().BeEmpty();
            harness.Errors.Should().BeEmpty("a refused click is not a fault");
        }

        [TestMethod]
        public async Task HandleToggleClickAsync_WhenEnginesAvailable_TogglesAndInvalidates()
        {
            // Arrange
            var harness = new Harness();
            harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).Returns(Task.CompletedTask);
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).ReturnsAsync(true);

            // Act
            await harness.Coordinator.HandleToggleClickAsync(SpamEngine);

            // Assert
            harness.Engines.Verify(x => x.ToggleEngineAsync(SpamEngine), Times.Once);
            harness.Invalidations.Should().Equal(new[] { SpamToggleControlId });
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeTrue("the refreshed state is cached before the control is invalidated");
            harness.Notifications.Should().BeEmpty();
            harness.Errors.Should().BeEmpty();
        }

        #endregion HandleToggleClickAsync — the observed boundary

        /// <summary>
        /// A coordinator wired to a strict <see cref="IAppItemEngines"/> mock and three recording
        /// sinks, so every test asserts on what the coordinator decided rather than on
        /// presentation.
        /// </summary>
        private sealed class Harness
        {
            internal Harness()
            {
                Coordinator = new EngineToggleStateCoordinator(
                    () => EnginesAvailable ? Engines.Object : null,
                    controlId =>
                    {
                        Invalidations.Add(controlId);
                        OnInvalidate?.Invoke(controlId);
                    },
                    message => Notifications.Add(message),
                    (message, exception) => Errors.Add(new LoggedError(message, exception))
                );
            }

            internal Mock<IAppItemEngines> Engines { get; } =
                new Mock<IAppItemEngines>(MockBehavior.Strict);

            internal EngineToggleStateCoordinator Coordinator { get; }

            /// <summary>
            /// When false the engines accessor yields null, modelling the pre-<c>SetGlobals</c>
            /// window.
            /// </summary>
            internal bool EnginesAvailable { get; set; } = true;

            /// <summary>
            /// An optional extra observer invoked from inside the invalidation sink, used by the
            /// ordering test to probe the cache at the exact moment Office would re-query.
            /// </summary>
            internal Action<string> OnInvalidate { get; set; }

            internal List<string> Invalidations { get; } = new List<string>();

            internal List<string> Notifications { get; } = new List<string>();

            internal List<LoggedError> Errors { get; } = new List<LoggedError>();
        }

        /// <summary>
        /// One observed fault, as delivered to the injected error-log delegate.
        /// </summary>
        private sealed class LoggedError
        {
            internal LoggedError(string message, Exception exception)
            {
                Message = message;
                Exception = exception;
            }

            internal string Message { get; }

            internal Exception Exception { get; }
        }
    }
}
