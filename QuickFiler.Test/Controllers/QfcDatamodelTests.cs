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
    [TestClass]
    public class QfcDatamodelTests
    {
        /// <summary>
        /// Builds the admission collaborator under test. Issue #731 finding 3 removed the settings
        /// and scoring-delegate constructor parameters, so no settings or globals arrangement is
        /// needed here any more: admission is provably independent of both.
        /// </summary>
        private static QfcRemainingQueueAdmission CreateQueueAdmission(
            IList<MailItem> added,
            IList<MailItem> hooked
        )
        {
            return new QfcRemainingQueueAdmission(
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
            var admission = CreateQueueAdmission(added, hooked);

            // Act
            var queued = await admission.TryQueueAsync(mailItem, CancellationToken.None);

            // Assert
            queued.Should().BeTrue();
            added.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
            hooked.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
        }

        /// <summary>
        /// Scenario: reflect over <see cref="QfcRemainingQueueAdmission"/>'s single constructor and
        /// its declared fields. Expected outcome: neither declares a scoring delegate. This pins the
        /// issue #233 intent structurally, in place of the behavioural test that previously pinned
        /// it by passing a throwing scorer through a parameter that no longer exists.
        /// </summary>
        [TestMethod]
        public void QfcRemainingQueueAdmission_DeclaresNoScoringDelegate()
        {
            // Arrange
            const string Rationale =
                "issue #233: Threshold scoring belongs to dequeue-time enforcement.";

            // Act
            ConstructorInfo[] constructors = typeof(QfcRemainingQueueAdmission).GetConstructors(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            // Assert
            constructors.Should().ContainSingle(because: Rationale);
            constructors[0]
                .GetParameters()
                .Should()
                .NotContain(
                    parameter =>
                        parameter.ParameterType
                        == typeof(Func<MailItem, CancellationToken, Task<long>>),
                    because: Rationale
                );
            typeof(QfcRemainingQueueAdmission)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Should()
                .NotContain(
                    field =>
                        field.FieldType == typeof(Func<MailItem, CancellationToken, Task<long>>),
                    because: Rationale
                );
        }

        [TestMethod]
        public async Task DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive()
        {
            var model = CreateUninitializedDatamodel();
            var fake = new FakeTimeProvider();
            model.TimeProvider = fake;

            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(x => x.HighConfidenceThreshold).Returns(0.90);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.QfSettings).Returns(settings.Object);

            var worker = new BackgroundWorker();
            SetPrivateField(model, "_globals", globals.Object);
            SetPrivateField(model, "_worker", worker);
            SetPrivateField(model, "_masterQueue", new LockingLinkedList<MailItem>());
            // Issue #424: the source-active signal is the datamodel-owned liveness flag, not
            // BackgroundWorker.isRunning, which is dishonest for an async void DoWork handler.
            SetPrivateField(model, "_remainingLoadActive", true);

            Task<IList<MailItem>> pending = model.DequeueNextItemGroupAsync(1, 200);

            fake.Advance(TimeSpan.FromMilliseconds(200));
            await Task.Yield();
            pending
                .IsCompleted.Should()
                .BeFalse(
                    "the datamodel source-active signal must keep polling while the worker can still add candidates"
                );

            SetPrivateField(model, "_remainingLoadActive", false);
            fake.Advance(TimeSpan.FromMilliseconds(200));
            IList<MailItem> result = await pending;

            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var admission = CreateQueueAdmission(added, hooked);

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
            var admission = CreateQueueAdmission(added, hooked);

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
            var admission = CreateQueueAdmission(added, hooked);

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
            SetPrivateField(model, "_worker", worker);
            SetPrivateField(model, "_masterQueue", new LockingLinkedList<MailItem>()); // Count == 0
            // Issue #424: WaitForQueue now loops on the datamodel-owned producer-liveness flag
            // instead of BackgroundWorker.IsBusy, so the loop is driven through that flag.
            SetPrivateField(model, "_remainingLoadActive", true);

            var method = typeof(QfcDatamodel).GetMethod("WaitForQueue", NonPublicInstance);

            // Act
            var task = (Task)method.Invoke(model, new object[] { 1, CancellationToken.None });

            // Assert — loop is parked on the injected delay until advanced.
            task.IsCompleted.Should()
                .BeFalse("WaitForQueue must await the injected 200 ms delay, not wall-clock");

            // Release the loop: the producer goes idle, then advancing the clock completes the delay
            // so the loop re-checks its condition and exits.
            SetPrivateField(model, "_remainingLoadActive", false);
            fake.Advance(TimeSpan.FromMilliseconds(200));
            await task;
            task.IsCompleted.Should().BeTrue();
        }

        #endregion Issue #222 — Injectable time/delay seam

        #region Issue #446 — Top-folder propagation from the master-queue admission scorer

        /// <summary>
        /// Issue #446. <c>ScoreRemainingQueueMailItemAsync</c> must surface BOTH halves of the
        /// scorer result. The scoring service already returns a <c>(Score, TopFolder)</c> pair, but
        /// the datamodel discards the folder, so a later consumer has to re-score the same item.
        /// Scoring is driven through the <c>ScoringServiceFactory</c> seam added by [P1-T5] so no
        /// live Outlook COM is touched, as .claude/rules/general-unit-test.md UT4 requires.
        /// Issue #678 widened the seam to a third element, the initialised folder search handler;
        /// this test additionally asserts that third element is forwarded rather than dropped, which
        /// is the same discard defect one element to the right.
        /// </summary>
        [TestMethod]
        public async Task ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var mailItem = new Mock<MailItem>().Object;
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);

            const long ExpectedScore = 875L;
            const string ExpectedTopFolder = @"Inbox\Projects\Alpha";
            IFolderSearchHandler expectedHandler = new Mock<IFolderSearchHandler>().Object;

            var scoringService = new Mock<IFolderScoringService>(MockBehavior.Strict);
            scoringService
                .Setup(x =>
                    x.ScoreAsync(
                        mailItem,
                        It.IsAny<IApplicationGlobals>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync((ExpectedScore, ExpectedTopFolder, expectedHandler));

            SetPrivateField(model, "_globals", globals.Object);
            model.ScoringServiceFactory = () => scoringService.Object;

            // Act
            (long Score, string TopFolder, IFolderSearchHandler Handler) result =
                await InvokeScoreRemainingQueueMailItemAsync(model, mailItem);

            // Assert
            result
                .Score.Should()
                .Be(ExpectedScore, "the score half of the scorer result is already propagated");
            result
                .TopFolder.Should()
                .Be(
                    ExpectedTopFolder,
                    "the top-ranked folder the scorer already computed must reach the caller "
                        + "instead of being discarded and re-derived downstream"
                );
            result
                .Handler.Should()
                .BeSameAs(
                    expectedHandler,
                    "issue #678: the folder search handler the scoring pass already initialised "
                        + "must reach the caller instead of being discarded and re-initialised"
                );
        }

        private static Task<(
            long Score,
            string TopFolder,
            IFolderSearchHandler Handler
        )> InvokeScoreRemainingQueueMailItemAsync(QfcDatamodel model, MailItem mailItem)
        {
            var method = typeof(QfcDatamodel).GetMethod(
                "ScoreRemainingQueueMailItemAsync",
                NonPublicInstance
            );
            method
                .Should()
                .NotBeNull(
                    "ScoreRemainingQueueMailItemAsync should exist on QfcDatamodel as a private "
                        + "instance method"
                );
            return (Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>)
                method.Invoke(model, new object[] { mailItem, CancellationToken.None });
        }

        #endregion Issue #446 — Top-folder propagation from the master-queue admission scorer
    }
}
