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
    /// Issue #424 coverage for the datamodel-owned producer-liveness flag. Relocated verbatim from
    /// <c>QfcDatamodelTests.cs</c> so that file stays under the 500-line limit. Carries its own
    /// <c>CreateUninitializedDatamodel</c> / <c>SetPrivateField</c> helpers, following the existing
    /// duplication convention in <c>QfcInitEmailQueueZeroBatchTests.cs</c> (which duplicates the same
    /// two helpers rather than sharing a base class).
    /// </summary>
    [TestClass]
    public class QfcDatamodelLivenessTests
    {
        private const BindingFlags NonPublicInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Builds a <see cref="QfcDatamodel"/> without running its COM-bound constructors. Fields the
        /// code under test reads are assigned explicitly via <see cref="SetPrivateField"/>.
        /// </summary>
        private static QfcDatamodel CreateUninitializedDatamodel() =>
            (QfcDatamodel)FormatterServices.GetUninitializedObject(typeof(QfcDatamodel));

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull($"private field '{name}' should exist on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on
        /// the calling thread through the protected <c>OnDoWork</c>, so the privately subscribed
        /// <c>Worker_DoWork</c> runs to its first incomplete await before <c>InitEmailQueue</c>
        /// returns. Issue #950: this replaces the bounded waits on a thread-pool worker.
        /// </summary>
        private sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));
        }

        /// <summary>The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>.</summary>
        private static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();

        /// <summary>
        /// Queues posted continuations and runs them only on an explicit <see cref="Drain"/> call,
        /// on the creating thread. Drain runs only work already queued, plus work that work queues,
        /// and never blocks. Installed around <c>InitEmailQueue</c> by the tests that observe the
        /// loader's continuation, and restored in a <c>finally</c>.
        /// </summary>
        private sealed class DrainableSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> _callbacks =
                new Queue<Tuple<SendOrPostCallback, object>>();
            private readonly int _creatorThreadId = Environment.CurrentManagedThreadId;

            public override void Post(SendOrPostCallback d, object state) =>
                _callbacks.Enqueue(Tuple.Create(d, state));

            /// <summary>Runs every queued callback, including work queued while draining.</summary>
            internal void Drain()
            {
                Environment.CurrentManagedThreadId.Should().Be(_creatorThreadId);
                while (_callbacks.Count > 0)
                {
                    Tuple<SendOrPostCallback, object> callback = _callbacks.Dequeue();
                    callback.Item1(callback.Item2);
                }
            }
        }

        /// <summary>Globals wired for the high-confidence dequeue path.</summary>
        private static IApplicationGlobals CreateHighConfidenceGlobals()
        {
            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(x => x.HighConfidenceThreshold).Returns(0.90);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.QfSettings).Returns(settings.Object);
            return globals.Object;
        }

        /// <summary>
        /// Issue #424 regression test for the latent producer-liveness defect. <c>Worker_DoWork</c> is
        /// <c>async void</c>, so it returns at its first yielding await and
        /// <see cref="BackgroundWorker.IsBusy"/> goes false while
        /// <c>LoadRemainingEmailsToQueueAsync</c> is still producing. The dequeue gate's
        /// <c>sourceActive</c> signal consumed that dishonest value, so an empty queue was mistaken
        /// for an exhausted one and the gate returned an early partial batch. The datamodel-owned
        /// <c>volatile bool</c> flag makes the signal truthful.
        /// </summary>
        [TestMethod]
        public async Task DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var fake = new FakeTimeProvider();
            model.TimeProvider = fake;
            SetPrivateField(model, "_globals", CreateHighConfidenceGlobals());
            SetPrivateField(model, "_masterQueue", new LockingLinkedList<MailItem>());

            var loaderEntered = new TaskCompletionSource<bool>();
            var loaderRelease = new TaskCompletionSource<bool>();
            model.RemainingEmailLoader = async _ =>
            {
                loaderEntered.TrySetResult(true);
                return await loaderRelease.Task;
            };

            var worker = new SynchronousBackgroundWorker();
            model.WorkerStarter = StartSynchronously;

            // Act — the issue #244 zero-batch short-circuit is COM-free and starts the worker
            // through the issue #950 seam, which raises DoWork on this thread.
            model.InitEmailQueue(0, worker);

            loaderEntered
                .Task.IsCompleted.Should()
                .BeTrue("the synchronous starter must reach the injected RemainingEmailLoader");

            Task<IList<MailItem>> pending = model.DequeueNextItemGroupAsync(1, 200);
            fake.Advance(TimeSpan.FromMilliseconds(200));
            await Task.Yield();
            fake.Advance(TimeSpan.FromMilliseconds(200));
            await Task.Yield();

            // Assert
            pending
                .IsCompleted.Should()
                .BeFalse(
                    "the loader is still producing, so the gate must keep polling rather than treat "
                        + "an empty queue as an exhausted source and return an early partial batch"
                );

            // Cleanup — release the loader and let the dequeue drain on the honest signal.
            loaderRelease.SetResult(true);
            for (int i = 0; i < 20 && !pending.IsCompleted; i++)
            {
                fake.Advance(TimeSpan.FromMilliseconds(200));
                await Task.Yield();
            }

            pending
                .IsCompleted.Should()
                .BeTrue("once the loader completes, the gate exits on genuine exhaustion");
            (await pending).Should().BeEmpty();
        }

        /// <summary>Reads the issue #424 producer-liveness flag by reflection.</summary>
        private static bool ReadLivenessFlag(QfcDatamodel model)
        {
            var field = typeof(QfcDatamodel).GetField("_remainingLoadActive", NonPublicInstance);
            field.Should().NotBeNull("the datamodel must own the producer-liveness flag");
            return (bool)field.GetValue(model);
        }

        /// <summary>
        /// Starts the worker with a <c>RemainingEmailLoader</c> held open by
        /// <paramref name="release"/>. The issue #950 synchronous starter raises <c>DoWork</c> on
        /// this thread, so by the time <c>InitEmailQueue</c> returns the async void
        /// <c>Worker_DoWork</c> has entered the loader and returned at its first incomplete await.
        /// <paramref name="release"/> runs its continuations asynchronously, so a test that has
        /// installed <c>DrainableSynchronizationContext</c> observes the resumed loader only
        /// through <c>Drain</c>, never inline inside <c>SetResult</c>.
        /// </summary>
        private static QfcDatamodel StartHeldOpenLoader(
            Func<TaskCompletionSource<bool>, Task<bool>> loaderBody,
            out TaskCompletionSource<bool> release
        )
        {
            var model = CreateUninitializedDatamodel();
            var entered = new TaskCompletionSource<bool>();
            var localRelease = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            release = localRelease;

            model.RemainingEmailLoader = _ =>
            {
                entered.TrySetResult(true);
                return loaderBody(localRelease);
            };

            var worker = new SynchronousBackgroundWorker();
            model.WorkerStarter = StartSynchronously;
            model.InitEmailQueue(0, worker);

            entered
                .Task.IsCompleted.Should()
                .BeTrue("the synchronous starter must reach the injected loader before returning");
            return model;
        }

        /// <summary>
        /// AC 7: the flag stays true across the <c>async void</c> first-await boundary while the
        /// loader is still producing. Issue #950: the synchronous starter has already run
        /// <c>Worker_DoWork</c> to that boundary when <c>InitEmailQueue</c> returns, so the flag
        /// is read with no wait.
        /// </summary>
        [TestMethod]
        public void RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces()
        {
            // Arrange / Act
            QfcDatamodel model = StartHeldOpenLoader(
                signal => signal.Task,
                out TaskCompletionSource<bool> release
            );

            // Assert
            ReadLivenessFlag(model)
                .Should()
                .BeTrue(
                    "the producer is still live even though the async void handler already returned"
                );

            release.SetResult(true);
        }

        /// <summary>
        /// AC 7: the flag becomes false only after the loader completes — never before. Issue
        /// #950: the continuation that clears the flag is drained from a test-owned context.
        /// </summary>
        [TestMethod]
        public void RemainingLoadActive_AfterLoaderCompletes_BecomesFalse()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            var pump = new DrainableSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(pump);
            try
            {
                QfcDatamodel model = StartHeldOpenLoader(
                    signal => signal.Task,
                    out TaskCompletionSource<bool> release
                );
                ReadLivenessFlag(model).Should().BeTrue("the loader has not completed yet");

                // Act
                release.SetResult(true);
                pump.Drain();

                // Assert
                ReadLivenessFlag(model)
                    .Should()
                    .BeFalse(
                        "the finally around the awaited loader must clear the flag once it completes"
                    );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        /// <summary>
        /// AC 7: the <c>finally</c> clears the flag even when the loader throws. Issue #950: the
        /// faulted loader's continuation is drained from a test-owned context.
        /// </summary>
        [TestMethod]
        public void RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            var pump = new DrainableSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(pump);
            try
            {
                QfcDatamodel model = StartHeldOpenLoader(
                    async signal =>
                    {
                        await signal.Task;
                        throw new InvalidOperationException("loader failed");
                    },
                    out TaskCompletionSource<bool> release
                );
                ReadLivenessFlag(model).Should().BeTrue("the loader has not failed yet");

                // Act
                release.SetResult(true);
                pump.Drain();

                // Assert
                ReadLivenessFlag(model)
                    .Should()
                    .BeFalse(
                        "the finally must clear the flag on the throwing path too, or the gate would poll forever"
                    );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }
    }
}
