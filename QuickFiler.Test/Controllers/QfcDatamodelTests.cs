using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    [TestClass]
    public class QfcDatamodelTests
    {
        private static string ReadControllerSource(string fileName)
        {
            string path = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\QuickFiler\Controllers",
                    fileName
                )
            );
            return File.ReadAllText(path);
        }

        [TestMethod]
        public void ScoreRemainingQueueMailItemAsync_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndScore()
        {
            string source = ReadControllerSource("QfcDatamodel.cs");

            source
                .Should()
                .Contain(
                    "Probability debug [QfcDatamodel.LoadRemainingEmailsToQueueAsync (master-queue admission)]"
                );
            source.Should().Contain("Subject='{mailItem.Subject}'");
            source.Should().Contain("EntryID='{mailItem.EntryID}'");
            source.Should().Contain("Score={score.Score}");
        }

        private static QfcRemainingQueueAdmission CreateQueueAdmission(
            bool highConfidenceEnabled,
            double threshold,
            IList<MailItem> added,
            IList<MailItem> hooked,
            Func<MailItem, CancellationToken, Task<long>> scoreLoader
        )
        {
            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(highConfidenceEnabled);
            if (highConfidenceEnabled)
            {
                settings.SetupGet(x => x.HighConfidenceThreshold).Returns(threshold);
            }

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.QfSettings).Returns(settings.Object);

            return new QfcRemainingQueueAdmission(
                globals.Object,
                scoreLoader,
                added.Add,
                (mail, _) => hooked.Add(mail),
                _ => { }
            );
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsAndHooksWithoutScoring()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var admission = CreateQueueAdmission(
                highConfidenceEnabled: true,
                threshold: 0.90,
                added,
                hooked,
                (mail, _) =>
                    throw new AssertFailedException(
                        "Remaining-mail admission must not score before queue insertion."
                    )
            );

            // Act
            var queued = await admission.TryQueueAsync(mailItem, CancellationToken.None);

            // Assert
            queued.Should().BeTrue();
            added.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
            hooked.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_HighConfidenceEnabled_IgnoresThresholdAtAdmission()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var admission = CreateQueueAdmission(
                highConfidenceEnabled: true,
                threshold: 0.90,
                added,
                hooked,
                (mail, token) =>
                    throw new AssertFailedException(
                        "Threshold scoring belongs to dequeue-time enforcement."
                    )
            );

            // Act
            var queued = await admission.TryQueueAsync(mailItem, CancellationToken.None);

            // Assert
            queued.Should().BeTrue();
            added.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
            hooked.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
        }

        [TestMethod]
        public void DequeueNextItemGroupAsync_HighConfidenceMode_UsesStreamingGate()
        {
            string source = ReadControllerSource("QfcDatamodel.QueueProcessing.cs");

            source.Should().Contain("HighConfidenceModeEnabled");
            source.Should().Contain("QfcStreamingDequeueConfidenceGate");
            source.Should().Contain("DequeueAsync(quantity, timeOut, _token)");
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var admission = CreateQueueAdmission(
                highConfidenceEnabled: true,
                threshold: 0.90,
                added,
                hooked,
                (mail, token) =>
                    throw new AssertFailedException(
                        "Below-threshold candidates must reach the queue for dequeue-time filtering."
                    )
            );

            // Act
            var queued = await admission.TryQueueAsync(mailItem, CancellationToken.None);

            // Assert
            queued.Should().BeTrue();
            added.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
            hooked.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var admission = CreateQueueAdmission(
                highConfidenceEnabled: false,
                threshold: 0.90,
                added,
                hooked,
                (mail, token) =>
                    throw new AssertFailedException("Scoring should not run when disabled.")
            );

            // Act
            var queued = await admission.TryQueueAsync(mailItem, CancellationToken.None);

            // Assert
            queued.Should().BeTrue();
            added.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
            hooked.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
        }

        /// <summary>
        /// Issue #218 admission guard: a null remaining <see cref="MailItem"/> must not be
        /// scored, added to the queue, or hooked. Covers the null-guard return path in
        /// <c>QfcRemainingQueueAdmission.TryQueueAsync</c>.
        /// </summary>
        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var admission = CreateQueueAdmission(
                highConfidenceEnabled: true,
                threshold: 0.90,
                added,
                hooked,
                (mail, token) =>
                    throw new AssertFailedException("Scoring must not run for a null mail item.")
            );

            // Act
            var queued = await admission.TryQueueAsync(null, CancellationToken.None);

            // Assert
            queued.Should().BeFalse();
            added.Should().BeEmpty();
            hooked.Should().BeEmpty();
        }

        #region Issue #222 — Injectable time/delay seam (correctness-only; class is [ExcludeFromCodeCoverage])

        private const BindingFlags NonPublicInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Builds a <see cref="QfcDatamodel"/> without running its COM-bound constructors so a
        /// single private delay method can be exercised in isolation. Fields the method under test
        /// reads are assigned explicitly by each test via <see cref="SetPrivateField"/>.
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
        /// Issue #222 site 1: <c>ToggleOfflineMode(false)</c> must await the 5 ms delay through the
        /// injected <see cref="TimeProvider"/> seam, not wall-clock <c>Task.Delay</c>. With a
        /// <see cref="FakeTimeProvider"/> the returned task must stay incomplete until the clock is
        /// advanced by exactly 5 ms.
        /// </summary>
        [TestMethod]
        public async Task ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var fake = new FakeTimeProvider();
            model.TimeProvider = fake;

            var commandBars = new Mock<Microsoft.Office.Core.CommandBars>(MockBehavior.Loose);
            commandBars.Setup(x => x.ExecuteMso("ToggleOnline"));
            var explorer = new Mock<Explorer>(MockBehavior.Loose);
            explorer.SetupGet(x => x.CommandBars).Returns(commandBars.Object);
            SetPrivateField(model, "_activeExplorer", explorer.Object);

            var method = typeof(QfcDatamodel).GetMethod("ToggleOfflineMode", NonPublicInstance);

            // Act
            var task = (Task<bool>)method.Invoke(model, new object[] { false });

            // Assert — the delay is sourced from the injected seam, so it cannot complete yet.
            task.IsCompleted.Should()
                .BeFalse("the 5 ms delay must come from the injected TimeProvider, not wall-clock");
            fake.Advance(TimeSpan.FromMilliseconds(5));
            var result = await task;
            result.Should().BeFalse();
            commandBars.Verify(x => x.ExecuteMso("ToggleOnline"), Times.Once);
        }

        /// <summary>
        /// Issue #222 site 2: <c>WaitForQueue</c> must await the 200 ms poll delay through the
        /// injected <see cref="TimeProvider"/> seam. With a <see cref="FakeTimeProvider"/> the loop
        /// must remain pending until the clock advances by 200 ms, after which it re-evaluates its
        /// condition and exits (worker no longer busy).
        /// </summary>
        [TestMethod]
        public async Task WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var fake = new FakeTimeProvider();
            model.TimeProvider = fake;

            var worker = new BackgroundWorker();
            SetPrivateField(worker, "isRunning", true); // BackgroundWorker.IsBusy => true
            SetPrivateField(model, "_worker", worker);
            SetPrivateField(model, "_masterQueue", new LockingLinkedList<MailItem>()); // Count == 0

            var method = typeof(QfcDatamodel).GetMethod("WaitForQueue", NonPublicInstance);

            // Act
            var task = (Task)method.Invoke(model, new object[] { 1, CancellationToken.None });

            // Assert — loop is parked on the injected delay until advanced.
            task.IsCompleted.Should()
                .BeFalse("WaitForQueue must await the injected 200 ms delay, not wall-clock");

            // Release the loop: worker becomes idle, then advancing the clock completes the delay
            // so the loop re-checks its condition and exits.
            SetPrivateField(worker, "isRunning", false);
            fake.Advance(TimeSpan.FromMilliseconds(200));
            await task;
            task.IsCompleted.Should().BeTrue();
        }

        #endregion Issue #222 — Injectable time/delay seam
    }
}
