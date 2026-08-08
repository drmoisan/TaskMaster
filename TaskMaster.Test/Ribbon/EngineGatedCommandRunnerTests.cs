using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for issue #503. Before the fix, an engine-backed Explorer-ribbon command
    /// clicked before <c>AppItemEngines.InitAsync()</c> had populated
    /// <c>Globals.Engines.InboxEngines</c> dereferenced an engine that did not yet exist and threw
    /// out of an <c>async void</c> handler: <see cref="NullReferenceException"/> on the
    /// <c>Controller.SB</c> / <c>Controller.Triage</c> paths and
    /// <see cref="KeyNotFoundException"/> on the <c>TestSpam_Click</c> dictionary-indexer path.
    /// </summary>
    [TestClass]
    public class EngineGatedCommandRunnerTests
    {
        /// <summary>
        /// Builds an engines accessor over an <see cref="IAppItemEngines"/> whose
        /// <c>InboxEngines</c> is the supplied dictionary, modelling the #503 window when that
        /// dictionary is empty.
        /// </summary>
        private static Func<IAppItemEngines> CreateEnginesAccessor(
            ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>> inboxEngines
        )
        {
            var engines = new Mock<IAppItemEngines>();
            engines.SetupGet(x => x.InboxEngines).Returns(inboxEngines);
            return () => engines.Object;
        }

        /// <summary>
        /// Builds a dictionary already carrying a non-null engine under the supplied key, modelling
        /// state S2 (initialization complete, engine present).
        /// </summary>
        private static ConcurrentDictionary<
            string,
            IConditionalEngine<MailItemHelper>
        > CreateReadyEngines(string engineName)
        {
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            inboxEngines[engineName] = new Mock<IConditionalEngine<MailItemHelper>>().Object;
            return inboxEngines;
        }

        [TestMethod]
        public async Task RunAsync_WhenEngineNotReady_DoesNotThrowNullReferenceException()
        {
            // Arrange: the #503 window — InboxEngines is the empty ConcurrentDictionary created by
            // the AppItemEngines field initializer, so the "Spam" engine is absent.
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            var notifications = new List<string>();
            var gate = new EngineReadinessGate(CreateEnginesAccessor(inboxEngines));
            var runner = new EngineGatedCommandRunner(gate, notifications.Add);
            var invoked = false;

            // Act: the action body reproduces the Controller.SB null-dereference shape.
            Func<Task> act = () =>
                runner.RunAsync(
                    "TrainSpam",
                    () =>
                    {
                        invoked = true;
                        throw new NullReferenceException(
                            "the guard must never let this action run while the engine is absent"
                        );
                    }
                );

            // Assert
            await act.Should()
                .NotThrowAsync(
                    "a click during the initialization window must be a no-op, not an unhandled "
                        + "NullReferenceException on the message-pump synchronization context"
                );
            invoked
                .Should()
                .BeFalse("the engine dereference must never be evaluated when the gate is closed");
        }

        [TestMethod]
        public async Task RunAsync_WhenEngineNotReady_DoesNotThrowKeyNotFoundException()
        {
            // Arrange: same empty-dictionary window, exercised through the TestSpam_Click shape.
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            var notifications = new List<string>();
            var gate = new EngineReadinessGate(CreateEnginesAccessor(inboxEngines));
            var runner = new EngineGatedCommandRunner(gate, notifications.Add);
            var invoked = false;

            // Act: the action body reproduces the TestSpam_Click dictionary-indexer shape,
            // which throws KeyNotFoundException rather than NullReferenceException.
            Func<Task> act = () =>
                runner.RunAsync(
                    "TestSpam",
                    () =>
                    {
                        invoked = true;
                        var engine = inboxEngines[SpamBayes.GroupName];
                        return Task.FromResult(engine);
                    }
                );

            // Assert
            await act.Should()
                .NotThrowAsync(
                    "the indexer path in TestSpam_Click must also be suppressed during the "
                        + "initialization window rather than throwing KeyNotFoundException"
                );
            invoked
                .Should()
                .BeFalse("the indexer must never be evaluated when the gate is closed");
        }

        [TestMethod]
        public async Task RunAsync_WhenEngineNotReady_EmitsExactlyOneNotificationContainingControlIdAndEngineName()
        {
            // Arrange
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            var notifications = new List<string>();
            var gate = new EngineReadinessGate(CreateEnginesAccessor(inboxEngines));
            var runner = new EngineGatedCommandRunner(gate, notifications.Add);

            // Act
            await runner.RunAsync("TriageSetA", () => Task.CompletedTask);

            // Assert: exactly one "still loading" notice, naming both the control and its engine.
            notifications
                .Should()
                .ContainSingle("a blocked click must produce one notification, never zero or many");
            notifications[0].Should().Contain("TriageSetA").And.Contain("Triage");
        }

        [TestMethod]
        public async Task RunAsync_WhenEngineReady_InvokesActionExactlyOnce()
        {
            // Arrange
            var gate = new EngineReadinessGate(CreateEnginesAccessor(CreateReadyEngines("Spam")));
            var notifications = new List<string>();
            var runner = new EngineGatedCommandRunner(gate, notifications.Add);
            var invocationCount = 0;

            // Act
            await runner.RunAsync(
                "TrainSpam",
                () =>
                {
                    invocationCount++;
                    return Task.CompletedTask;
                }
            );

            // Assert: the ready path is unchanged from pre-fix behaviour (R6).
            invocationCount.Should().Be(1, "the ready path must run the action exactly once");
            notifications.Should().BeEmpty("a ready command must not emit a still-loading notice");
        }

        [TestMethod]
        public async Task RunAsync_WhenEngineReady_AwaitsActionToCompletion()
        {
            // Arrange: a TaskCompletionSource the test completes synchronously, so completion is
            // observed with no timing dependency and no sleep or delay of any kind.
            var gate = new EngineReadinessGate(CreateEnginesAccessor(CreateReadyEngines("Triage")));
            var notifications = new List<string>();
            var runner = new EngineGatedCommandRunner(gate, notifications.Add);
            var completionSource = new TaskCompletionSource<bool>();
            var actionCompleted = false;

            // Act
            var runTask = runner.RunAsync(
                "TriageSetB",
                async () =>
                {
                    await completionSource.Task;
                    actionCompleted = true;
                }
            );
            runTask.IsCompleted.Should().BeFalse("the runner must still be awaiting the action");
            completionSource.SetResult(true);
            await runTask;

            // Assert
            actionCompleted
                .Should()
                .BeTrue("RunAsync must await the supplied action to completion, not fire-and-forget");
        }

        [TestMethod]
        public async Task RunAsync_WhenActionThrows_PropagatesException()
        {
            // Arrange
            var gate = new EngineReadinessGate(CreateEnginesAccessor(CreateReadyEngines("Spam")));
            var notifications = new List<string>();
            var runner = new EngineGatedCommandRunner(gate, notifications.Add);

            // Act
            Func<Task> act = () =>
                runner.RunAsync(
                    "TrainHam",
                    () => throw new InvalidOperationException("engine failure")
                );

            // Assert: the guard suppresses invocation, never errors. It must not swallow.
            await act.Should()
                .ThrowAsync<InvalidOperationException>(
                    "an exception raised by a ready action must propagate unchanged (fail-fast)"
                )
                .WithMessage("engine failure");
        }

        [TestMethod]
        public async Task RunAsync_WithUnknownControlId_DoesNotInvokeAction()
        {
            // Arrange: engines are fully loaded, so only the unknown id can block the call.
            var gate = new EngineReadinessGate(CreateEnginesAccessor(CreateReadyEngines("Spam")));
            var notifications = new List<string>();
            var runner = new EngineGatedCommandRunner(gate, notifications.Add);
            var invoked = false;

            // Act
            await runner.RunAsync(
                "NotAnEngineBackedControl",
                () =>
                {
                    invoked = true;
                    return Task.CompletedTask;
                }
            );

            // Assert
            invoked.Should().BeFalse("an id the catalog does not own must never run an action");
            notifications.Should().ContainSingle();
            notifications[0]
                .Should()
                .Contain("NotAnEngineBackedControl")
                .And.Contain(
                    "(unmapped)",
                    "an unmapped id must be reported as such rather than named with a false engine"
                );
        }

        [TestMethod]
        public async Task RunAsync_WithNullAction_ThrowsArgumentNullException()
        {
            // Arrange
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            var notifications = new List<string>();
            var gate = new EngineReadinessGate(CreateEnginesAccessor(inboxEngines));
            var runner = new EngineGatedCommandRunner(gate, notifications.Add);

            // Act
            Func<Task> act = () => runner.RunAsync("TrainSpam", null);

            // Assert: the precondition is checked before any readiness evaluation, so no
            // notification is emitted.
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("action");
            notifications
                .Should()
                .BeEmpty("the null-action precondition must be checked before the gate is queried");
        }

        [TestMethod]
        public void IsCommandEnabled_WhenEngineNotReady_ReturnsFalse()
        {
            // Arrange: the #503 window.
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            var gate = new EngineReadinessGate(CreateEnginesAccessor(inboxEngines));
            var runner = new EngineGatedCommandRunner(gate, _ => { });

            // Act
            var enabled = runner.IsCommandEnabled("TrainSpam");

            // Assert
            enabled.Should().BeFalse("Office must render the button disabled during initialization");
        }

        [TestMethod]
        public void IsCommandEnabled_WhenEngineReady_ReturnsTrue()
        {
            // Arrange
            var gate = new EngineReadinessGate(CreateEnginesAccessor(CreateReadyEngines("Triage")));
            var runner = new EngineGatedCommandRunner(gate, _ => { });

            // Act
            var enabled = runner.IsCommandEnabled("ClearTriage");

            // Assert
            enabled.Should().BeTrue("the button must become enabled once its engine has loaded");
        }

        [TestMethod]
        public void IsCommandEnabled_WithUnknownControlId_ReturnsFalse()
        {
            // Arrange
            var gate = new EngineReadinessGate(CreateEnginesAccessor(CreateReadyEngines("Spam")));
            var runner = new EngineGatedCommandRunner(gate, _ => { });

            // Act
            var enabled = runner.IsCommandEnabled("NotAnEngineBackedControl");

            // Assert
            enabled
                .Should()
                .BeFalse("the callback must not claim ownership of a control it does not map");
        }

        [TestMethod]
        public void Constructor_WithNullGate_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () => new EngineGatedCommandRunner(null, _ => { });

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("gate");
        }

        [TestMethod]
        public void Constructor_WithNullNotificationSink_ThrowsArgumentNullException()
        {
            // Arrange
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            var gate = new EngineReadinessGate(CreateEnginesAccessor(inboxEngines));

            // Act
            Action act = () => new EngineGatedCommandRunner(gate, null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("notifyNotReady");
        }
    }
}
