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
        /// Bounded, event-driven wait for a state transition. This is not a fixed sleep: it returns
        /// as soon as the condition holds, and fails the test with a clear message if it never does.
        /// Required because <c>BackgroundWorker</c> clears <c>isRunning</c> from an asynchronously
        /// posted completion, so the transition is not observable synchronously (the same race the
        /// remarks on <see cref="QfcInitEmailQueueZeroBatchTests"/> document).
        /// </summary>
        private static void WaitForState(Func<bool> condition, string because)
        {
            SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5)).Should().BeTrue(because);
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

            var worker = new BackgroundWorker();

            // Act — the issue #244 zero-batch short-circuit is COM-free and still starts the worker.
            model.InitEmailQueue(0, worker);

            loaderEntered
                .Task.Wait(TimeSpan.FromSeconds(5))
                .Should()
                .BeTrue("the started worker must reach the injected RemainingEmailLoader");
            WaitForState(
                () => !worker.IsBusy,
                "the async void Worker_DoWork returns at its first await, so IsBusy goes false "
                    + "while the loader is still producing"
            );

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
        /// <paramref name="release"/>, and returns once the worker has entered the loader and
        /// <c>BackgroundWorker.IsBusy</c> has gone false at the async-void first-await boundary.
        /// </summary>
        private static QfcDatamodel StartHeldOpenLoader(
            Func<TaskCompletionSource<bool>, Task<bool>> loaderBody,
            out TaskCompletionSource<bool> release
        )
        {
            var model = CreateUninitializedDatamodel();
            var entered = new TaskCompletionSource<bool>();
            var localRelease = new TaskCompletionSource<bool>();
            release = localRelease;

            model.RemainingEmailLoader = _ =>
            {
                entered.TrySetResult(true);
                return loaderBody(localRelease);
            };

            var worker = new BackgroundWorker();
            model.InitEmailQueue(0, worker);

            entered
                .Task.Wait(TimeSpan.FromSeconds(5))
                .Should()
                .BeTrue("the started worker must reach the injected loader");
            WaitForState(
                () => !worker.IsBusy,
                "async void Worker_DoWork returns at its first await"
            );
            return model;
        }

        /// <summary>
        /// AC 7: the flag stays true across the <c>async void</c> first-await boundary while the
        /// loader is still producing — precisely where <c>BackgroundWorker.IsBusy</c> has already
        /// gone false.
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
        /// AC 7: the flag becomes false only after the loader completes — never before.
        /// </summary>
        [TestMethod]
        public void RemainingLoadActive_AfterLoaderCompletes_BecomesFalse()
        {
            // Arrange / Act
            QfcDatamodel model = StartHeldOpenLoader(
                signal => signal.Task,
                out TaskCompletionSource<bool> release
            );
            ReadLivenessFlag(model).Should().BeTrue("the loader has not completed yet");

            release.SetResult(true);

            // Assert
            WaitForState(
                () => !ReadLivenessFlag(model),
                "the finally around the awaited loader must clear the flag once it completes"
            );
        }

        /// <summary>
        /// AC 7: the <c>finally</c> clears the flag even when the loader throws.
        /// </summary>
        [TestMethod]
        public void RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally()
        {
            // Arrange / Act
            QfcDatamodel model = StartHeldOpenLoader(
                async signal =>
                {
                    await signal.Task;
                    throw new InvalidOperationException("loader failed");
                },
                out TaskCompletionSource<bool> release
            );
            ReadLivenessFlag(model).Should().BeTrue("the loader has not failed yet");

            release.SetResult(true);

            // Assert
            WaitForState(
                () => !ReadLivenessFlag(model),
                "the finally must clear the flag on the throwing path too, or the gate would poll forever"
            );
        }
    }
}
