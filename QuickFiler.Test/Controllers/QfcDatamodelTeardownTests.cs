using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #791 AC2 coverage for the datamodel side of the Cancel teardown: the loader-quiesce
    /// boundary, the relocated admission guard, and a repeat-safe <c>Cleanup()</c>.
    /// <para>
    /// Carries its own <c>CreateUninitializedDatamodel</c> / <c>SetPrivateField</c> helpers,
    /// following the existing duplication convention documented on
    /// <c>QfcDatamodelLivenessTests</c>. Deterministic — <see cref="FakeTimeProvider"/> for all
    /// time, mocked <see cref="MailItem"/>, no COM, no sleeps, no wall-clock waits.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcDatamodelTeardownTests
    {
        private const BindingFlags NonPublicInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Builds a <see cref="QfcDatamodel"/> without running its COM-bound constructors. Fields
        /// the code under test reads are assigned explicitly via <see cref="SetPrivateField"/>.
        /// </summary>
        private static QfcDatamodel CreateUninitializedDatamodel() =>
            (QfcDatamodel)FormatterServices.GetUninitializedObject(typeof(QfcDatamodel));

        private static void SetPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull($"private field '{name}' should exist on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private static object GetPrivateField(object target, string name)
        {
            FieldInfo field = target.GetType().GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull($"private field '{name}' should exist on {target.GetType().Name}");
            return field.GetValue(target);
        }

        /// <summary>
        /// Bounded, event-driven wait for a state transition. This is not a fixed sleep: it returns
        /// as soon as the condition holds and fails the test with a clear message if it never does.
        /// Required because <c>Worker_DoWork</c> is <c>async void</c> and runs on the
        /// <see cref="BackgroundWorker"/> thread, so the field assignment it performs is not
        /// observable synchronously from the calling thread.
        /// </summary>
        private static void WaitForState(Func<bool> condition, string because) =>
            SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5)).Should().BeTrue(because);

        /// <summary>
        /// AC2, the reported crash. Once <c>Cleanup()</c> has nulled <c>_masterQueue</c> and
        /// <c>_moveMonitor</c>, the still-running loader reached this method and constructed
        /// <c>QfcRemainingQueueAdmission</c> over method groups on those null instances, which raises
        /// <see cref="ArgumentException"/> "Delegate to an instance method cannot have null 'this'".
        /// The guard must return <see langword="false"/> at the accept point instead of throwing at
        /// the throw point.
        /// </summary>
        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing()
        {
            // Arrange — exactly the post-Cleanup field state.
            QfcDatamodel model = CreateUninitializedDatamodel();
            SetPrivateField(model, "_masterQueue", null);
            SetPrivateField(model, "_moveMonitor", null);
            MailItem mailItem = new Mock<MailItem>().Object;

            // Act
            Func<Task<bool>> act = () =>
                model.TryQueueRemainingMailItemAsync(mailItem, CancellationToken.None);

            // Assert
            bool queued = false;
            Func<Task> invoke = async () => queued = await act();
            await invoke
                .Should()
                .NotThrowAsync(
                    "a released field must be a refusal at the accept point, not a throw"
                );
            queued.Should().BeFalse("nothing can be queued once the master queue is released");
        }

        /// <summary>
        /// AC2: a loader that has already finished is reported as completed and the quiesce returns
        /// without consuming any of the bound. The fake clock is never advanced, so a wait on the
        /// bound could not complete: only the completion path can finish this call.
        /// </summary>
        [TestMethod]
        public async Task QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout()
        {
            // Arrange
            QfcDatamodel model = CreateUninitializedDatamodel();
            var logs = new List<string>();
            model.TimeProvider = new FakeTimeProvider();
            model.QuiesceDebugLog = logs.Add;
            model.TokenSource = new CancellationTokenSource();
            SetPrivateField(model, "_remainingLoadTask", Task.CompletedTask);

            // Act
            Task pending = model.QuiesceLoaderAsync(TimeSpan.FromSeconds(5));
            await pending;

            // Assert
            pending.IsCompleted.Should().BeTrue("the loader had already finished");
            logs.Should()
                .ContainSingle(line => line.Contains("Loader quiesce completed"))
                .Which.Should()
                .NotBeNullOrEmpty();
        }

        /// <summary>
        /// AC2: a loader still in flight is bounded out, reported, and never raised. The timeout case
        /// is what makes the Cancel path safe to await: it always returns, so a hung loader cannot
        /// stall the teardown.
        /// </summary>
        [TestMethod]
        public async Task QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs()
        {
            // Arrange
            QfcDatamodel model = CreateUninitializedDatamodel();
            var logs = new List<string>();
            var fake = new FakeTimeProvider();
            var hanging = new TaskCompletionSource<bool>();
            model.TimeProvider = fake;
            model.QuiesceDebugLog = logs.Add;
            model.TokenSource = new CancellationTokenSource();
            SetPrivateField(model, "_remainingLoadTask", hanging.Task);

            // Act — advancing the fake clock is the only thing that releases the bound, so the test
            // carries no wall-clock wait and no sleep.
            Task pending = model.QuiesceLoaderAsync(TimeSpan.FromSeconds(5));
            pending
                .IsCompleted.Should()
                .BeFalse("the loader has not completed and the bound is open");

            fake.Advance(TimeSpan.FromSeconds(6));
            Func<Task> act = () => pending;

            // Assert
            await act.Should().NotThrowAsync("a bounded-out loader is reported, never raised");
            logs.Should().ContainSingle(line => line.Contains("Loader quiesce timed out"));
            hanging.TrySetResult(false);
        }

        /// <summary>
        /// AC2: a second Cancel, or a Cancel after a partially failed launch, reaches
        /// <c>Cleanup()</c> with <c>_globals</c> and <c>_moveMonitor</c> already released. The
        /// unguarded dereferences raised <see cref="NullReferenceException"/> there, which aborted
        /// the teardown before the release callback.
        /// </summary>
        [TestMethod]
        public void Cleanup_CalledTwice_DoesNotThrow()
        {
            // Arrange
            QfcDatamodel model = CreateUninitializedDatamodel();
            using (var worker = new BackgroundWorker { WorkerSupportsCancellation = true })
            using (var tokenSource = new CancellationTokenSource())
            {
                SetPrivateField(model, "_globals", null);
                SetPrivateField(model, "_moveMonitor", null);
                SetPrivateField(model, "_tokenSource", tokenSource);
                SetPrivateField(model, "_worker", worker);

                // Act — `System.Action` is required: a bare `Action` is CS0104-ambiguous with
                // Microsoft.Office.Interop.Outlook.Action in this namespace.
                System.Action act = () =>
                {
                    model.Cleanup();
                    model.Cleanup();
                };

                // Assert
                act.Should()
                    .NotThrow("repeat teardown must be inert, not a fault on released fields");
            }
        }

        /// <summary>
        /// AC2 capture pin. <c>Worker_DoWork</c> is <c>async void</c> and retained no handle to the
        /// loader task, so nothing could await it. Capturing the task into <c>_remainingLoadTask</c>
        /// is what gives <c>QuiesceLoaderAsync</c> something to wait on.
        /// </summary>
        [TestMethod]
        public void Worker_DoWork_CapturesRemainingLoadTask()
        {
            // Arrange
            QfcDatamodel model = CreateUninitializedDatamodel();
            var loaderEntered = new TaskCompletionSource<bool>();
            var loaderRelease = new TaskCompletionSource<bool>();
            model.RemainingEmailLoader = async _ =>
            {
                loaderEntered.TrySetResult(true);
                return await loaderRelease.Task;
            };

            using (var worker = new BackgroundWorker())
            {
                // Act — the issue #244 zero-batch short-circuit is COM-free and still starts the
                // worker, which is the only path that reaches Worker_DoWork without live Outlook.
                model.InitEmailQueue(0, worker);
                loaderEntered
                    .Task.Wait(TimeSpan.FromSeconds(5))
                    .Should()
                    .BeTrue("the started worker must reach the injected RemainingEmailLoader");

                // Assert
                WaitForState(
                    () => GetPrivateField(model, "_remainingLoadTask") != null,
                    "the loader task must be captured before it is awaited, so the Cancel path has "
                        + "a handle to quiesce"
                );

                loaderRelease.TrySetResult(true);
            }
        }
    }
}
