using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Test.TestSupport;
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
        /// <para>
        /// Issue #968: every step waits on an explicit signal instead of a clock advance followed by
        /// a scheduler yield. <see cref="ArmingFakeTimeProvider.Armed"/> proves the gate armed its
        /// next wait; the dequeue task itself is the completion signal; the production awaits are
        /// registered with no synchronization context installed, so the loader's completion clears
        /// the flag inline and is read back before the final advance. No retry loop remains.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var clock = new ArmingFakeTimeProvider();
            model.TimeProvider = clock;
            SetPrivateField(model, "_globals", CreateHighConfidenceGlobals());
            SetPrivateField(model, "_masterQueue", new LockingLinkedList<MailItem>());

            var loaderEntered = new TaskCompletionSource<bool>();
            var loaderRelease = new TaskCompletionSource<bool>();
            model.RemainingEmailLoader = async _ =>
            {
                loaderEntered.TrySetResult(true);
                return await loaderRelease.Task;
            };

            using (var worker = new SynchronousBackgroundWorker())
            {
                model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;
                Task<IList<MailItem>> pending;
                using (NoSynchronizationContext())
                {
                    // The issue #244 zero-batch short-circuit is COM-free and starts the worker
                    // through the issue #950 seam, which raises DoWork on this thread.
                    model.InitEmailQueue(0, worker);
                    loaderEntered
                        .Task.IsCompleted.Should()
                        .BeTrue(
                            "the synchronous starter must reach the injected RemainingEmailLoader"
                        );
                    pending = model.DequeueNextItemGroupAsync(1, 200);
                }

                clock
                    .Armed.IsCompleted.Should()
                    .BeTrue(
                        "the gate arms its first empty-queue wait before the dequeue call returns"
                    );
                clock.ReArm();

                // Act — the first wait expires while the loader is still producing.
                clock.Advance(TimeSpan.FromMilliseconds(200));
                Task first = await Task.WhenAny(clock.Armed, pending);

                // Assert
                first
                    .Should()
                    .BeSameAs(
                        clock.Armed,
                        "the loader is still producing, so the gate must arm a second wait rather than "
                            + "treat an empty queue as an exhausted source and return an early partial batch"
                    );
                pending.IsCompleted.Should().BeFalse("the gate re-armed instead of returning");

                // Cleanup — complete the loader; with no captured context its continuations run
                // inline and clear the flag before this call returns.
                using (NoSynchronizationContext())
                {
                    loaderRelease.SetResult(true);
                }

                ReadLivenessFlag(model)
                    .Should()
                    .BeFalse("the loader's completion must clear the flag before the next poll");
                clock.Advance(TimeSpan.FromMilliseconds(200));
                (await pending)
                    .Should()
                    .BeEmpty("once the loader completes, the gate exits on genuine exhaustion");
            }
        }

        /// <summary>Reads the issue #424 producer-liveness flag by reflection.</summary>
        private static bool ReadLivenessFlag(QfcDatamodel model)
        {
            var field = typeof(QfcDatamodel).GetField("_remainingLoadActive", NonPublicInstance);
            field.Should().NotBeNull("the datamodel must own the producer-liveness flag");
            return (bool)field.GetValue(model);
        }

        /// <summary>
        /// Issue #968. Clears <see cref="SynchronizationContext.Current"/> on the calling thread for the
        /// lifetime of the returned scope and restores the previous value on dispose, so every
        /// production await registered inside the scope captures no context and its continuation runs
        /// inline on the completing thread. The scope body must contain no <c>await</c>: the restore
        /// has to run on the same thread that took the scope.
        /// </summary>
        private static IDisposable NoSynchronizationContext() => new SynchronizationContextScope();

        private sealed class SynchronizationContextScope : IDisposable
        {
            private readonly SynchronizationContext _previous = SynchronizationContext.Current;

            internal SynchronizationContextScope() =>
                SynchronizationContext.SetSynchronizationContext(null);

            public void Dispose() => SynchronizationContext.SetSynchronizationContext(_previous);
        }

        /// <summary>
        /// Starts <paramref name="worker"/> with a <c>RemainingEmailLoader</c> held open by
        /// <paramref name="release"/>. The issue #950 synchronous starter raises <c>DoWork</c> on
        /// this thread, so by the time <c>InitEmailQueue</c> returns the async void
        /// <c>Worker_DoWork</c> has entered the loader and returned at its first incomplete await.
        /// <paramref name="release"/> runs its continuations asynchronously, so a test that has
        /// installed <c>DrainableSynchronizationContext</c> observes the resumed loader only
        /// through <c>Drain</c>, never inline inside <c>SetResult</c>. The caller owns and disposes
        /// the worker (issue #968, folding issue #972 item 4).
        /// </summary>
        private static QfcDatamodel StartHeldOpenLoader(
            SynchronousBackgroundWorker worker,
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

            model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;
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
            using (var worker = new SynchronousBackgroundWorker())
            {
                // Arrange / Act
                QfcDatamodel model = StartHeldOpenLoader(
                    worker,
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
                using (var worker = new SynchronousBackgroundWorker())
                {
                    QfcDatamodel model = StartHeldOpenLoader(
                        worker,
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
                using (var worker = new SynchronousBackgroundWorker())
                {
                    QfcDatamodel model = StartHeldOpenLoader(
                        worker,
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
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }
    }
}
