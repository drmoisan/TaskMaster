using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class QfcHomeControllerMetricsTests
    {
        /// <summary>
        /// Regression test for Issue #97: QuickFileMetrics_WRITE must not throw a
        /// NullReferenceException when GetCalendar returns null because the "Email Time" Outlook
        /// calendar subfolder does not exist. The fixture's calendar root enumerates no subfolders,
        /// so GetCalendar returns null, and GetMoveDiagnostics returns an empty array so the write
        /// iterates zero items and touches no file.
        /// </summary>
        [TestMethod]
        public void QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow()
        {
            var (controller, _) = BuildLooseMetricsController();

            System.Action act = () => controller.QuickFileMetrics_WRITE("test-metrics.txt");

            act.Should().NotThrow();
        }

        /// <summary>
        /// Regression test for Issue #97: the metrics path must not throw a NullReferenceException
        /// when the olAppointment ref parameter is null. The fixture supplies no MyDocuments entry,
        /// which is the arrange the original form of this test used.
        /// </summary>
        [TestMethod]
        public void GetMoveDiagnostics_NullAppointment_DoesNotThrow()
        {
            var (controller, _) = BuildLooseMetricsController(withMyDocuments: false);

            System.Action act = () => controller.QuickFileMetrics_WRITE("test-metrics-2.txt");

            act.Should().NotThrow();
        }

        #region Issue #222 — Injectable time/delay seam tests

        private static void SetPrivateField(object target, string name, object value)
        {
            target
                .GetType()
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        /// <summary>
        /// Builds a controller with loose mocks wired so the metrics-write path runs to the
        /// <c>GetMoveDiagnostics</c> call without touching live COM or the filesystem:
        /// SpecialFolders has MyDocuments (no early return), the calendar resolves to null, and
        /// GetMoveDiagnostics returns an empty array (so the metrics producer writes nothing).
        /// </summary>
        private static (
            QfcHomeController controller,
            Mock<IQfcCollectionController> groups
        ) BuildLooseMetricsController(string[] diagnostics = null, bool withMyDocuments = true)
        {
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Loose);

            var specialFolders = new ConcurrentDictionary<string, string>();
            if (withMyDocuments)
            {
                specialFolders["MyDocuments"] = FakeDocumentsRoot;
            }
            var mockFs = new Mock<IFileSystemFolderPaths>(MockBehavior.Loose);
            mockFs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);

            var mockFolders = new Mock<Folders>(MockBehavior.Loose);
            mockFolders
                .Setup(x => x.GetEnumerator())
                .Returns(() => new ArrayList().GetEnumerator());
            var mockCalendarRoot = new Mock<Folder>(MockBehavior.Loose);
            mockCalendarRoot.SetupGet(x => x.Folders).Returns(mockFolders.Object);
            var mockSession = new Mock<NameSpace>(MockBehavior.Loose);
            mockSession
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderCalendar))
                .Returns(mockCalendarRoot.Object);
            var mockOlApp = new Mock<Outlook.Application>(MockBehavior.Loose);
            mockOlApp.SetupGet(x => x.Session).Returns(mockSession.Object);
            var mockOl = new Mock<IOlObjects>(MockBehavior.Loose);
            mockOl.SetupGet(x => x.App).Returns(mockOlApp.Object);
            mockGlobals.SetupGet(x => x.Ol).Returns(mockOl.Object);

            var mockGroups = new Mock<IQfcCollectionController>(MockBehavior.Loose);
            mockGroups.SetupGet(x => x.EmailsToMove).Returns(1);
            mockGroups
                .Setup(x =>
                    x.GetMoveDiagnostics(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<double>(),
                        It.IsAny<string>(),
                        It.IsAny<DateTime>(),
                        ref It.Ref<AppointmentItem>.IsAny
                    )
                )
                .Returns(diagnostics ?? Array.Empty<string>());
            var mockFormController = new Mock<IQfcFormController>(MockBehavior.Loose);
            mockFormController.SetupGet(x => x.Groups).Returns(mockGroups.Object);

            mockGlobals.SetupGet(x => x.AF.CancelToken).Returns(CancellationToken.None);

            var controller = new QfcHomeController(mockGlobals.Object, () => { });
            controller.CreateCancellationToken();
            // Replace the production file writer with a no-op. The default seam value is
            // FileIO2.WriteTextFileAsync, which probes a real path and retries 100 times over ten
            // seconds when the folder is absent; a unit test must not touch the filesystem or wait
            // on wall-clock time. Tests that assert on the flush override this with a capturing
            // delegate of their own.
            controller.MetricsFileWriter = (filename, lines, folderRoot, token) =>
                Task.CompletedTask;
            SetPrivateField(controller, "_formController", mockFormController.Object);
            SetPrivateField(controller, "_stopWatchMoved", new Stopwatch());
            return (controller, mockGroups);
        }

        /// <summary>
        /// Builds a stopped stopwatch reporting a fixed, explicitly set elapsed interval. The
        /// internal tick field is assigned directly rather than started and then stopped, because a
        /// wall-clock start/stop pair does not guarantee a non-zero elapsed value and would make
        /// every assertion derived from it time-dependent.
        /// </summary>
        private static Stopwatch StoppedStopwatchWithElapsed(int seconds)
        {
            var stopwatch = new Stopwatch();
            SetPrivateField(stopwatch, "elapsed", Stopwatch.Frequency * (long)seconds);
            return stopwatch;
        }

        /// <summary>
        /// The duration must come from the moved-items stopwatch, not from the session stopwatch.
        /// The pre-fix source reads the freshly constructed session stopwatch and passes zero.
        /// </summary>
        [TestMethod]
        public async Task WriteMetricsAsync_ReadsMovedStopwatchForDuration()
        {
            var (controller, groups) = BuildLooseMetricsController();
            SetPrivateField(controller, "_stopWatchMoved", StoppedStopwatchWithElapsed(30));
            SetPrivateField(controller, "_stopWatch", new Stopwatch());

            await controller.WriteMetricsAsync("metrics.csv");

            groups.Verify(
                x =>
                    x.GetMoveDiagnostics(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.Is<double>(d => d > 0),
                        It.IsAny<string>(),
                        It.IsAny<DateTime>(),
                        ref It.Ref<AppointmentItem>.IsAny
                    ),
                Times.Once
            );
        }

        /// <summary>
        /// The metrics file is machine-read, so its numeric fields must not follow the operator's
        /// locale. Under de-DE the pre-fix source renders the minutes field with a decimal comma,
        /// which would add a field to the CSV row. The assertion is independent of the elapsed
        /// value, so it needs no clock read.
        /// </summary>
        [TestMethod]
        public async Task WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                var (controller, groups) = BuildLooseMetricsController();
                SetPrivateField(controller, "_stopWatchMoved", StoppedStopwatchWithElapsed(30));
                SetPrivateField(controller, "_stopWatch", new Stopwatch());

                await controller.WriteMetricsAsync("metrics.csv");

                groups.Verify(
                    x =>
                        x.GetMoveDiagnostics(
                            It.IsAny<string>(),
                            It.Is<string>(text => !text.Contains(",")),
                            It.IsAny<double>(),
                            It.IsAny<string>(),
                            It.IsAny<DateTime>(),
                            ref It.Ref<AppointmentItem>.IsAny
                        ),
                    Times.Once
                );
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Builds a deterministic <see cref="FakeTimeProvider"/> fixed to a known instant. Expected
        /// values are derived from the fake's own GetLocalNow().LocalDateTime so the test mirrors the
        /// exact production computation (GetLocalNow().LocalDateTime) regardless of host time zone.
        /// Moq cannot mock the non-virtual GetLocalNow(); FakeTimeProvider is the prescribed seam.
        /// </summary>
        private static FakeTimeProvider FixedClock() =>
            new FakeTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));

        /// <summary>
        /// Issue #222 sites 5-7: <c>WriteMetricsAsync</c> must source its date/time stamps and the
        /// OlEndTime from the injected <see cref="TimeProvider"/>. The formatted dataLineBeg
        /// ("MM/dd/yyyy","hh:mm") and the OlEndTime passed to GetMoveDiagnostics must reflect the
        /// injected clock, not wall-clock. Fails if the seam is bypassed.
        /// </summary>
        [TestMethod]
        public async Task WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps()
        {
            // Arrange
            var (controller, groups) = BuildLooseMetricsController();
            SetPrivateField(controller, "_stopWatch", new Stopwatch());
            // The duration read moves to the moved-items stopwatch, so this field must be
            // populated or the production path dereferences null.
            SetPrivateField(controller, "_stopWatchMoved", new Stopwatch());
            var fake = FixedClock();
            controller.TimeProvider = fake;
            var expectedLocal = fake.GetLocalNow().LocalDateTime;
            var expectedDataLineBeg =
                expectedLocal.ToString("MM/dd/yyyy") + "," + expectedLocal.ToString("hh:mm") + ",";

            // Act
            await controller.WriteMetricsAsync("metrics.csv");

            // Assert
            groups.Verify(
                x =>
                    x.GetMoveDiagnostics(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<double>(),
                        expectedDataLineBeg,
                        expectedLocal,
                        ref It.Ref<AppointmentItem>.IsAny
                    ),
                Times.Once
            );
        }

        /// <summary>
        /// Issue #222 site 4: <c>QuickFileMetrics_WRITE</c> must build its data line and endTime from
        /// the injected <see cref="TimeProvider"/>. The dataLineBeg ("MM/dd/yyyy","hh:mm") and the
        /// endTime passed to GetMoveDiagnostics must reflect the injected clock. Fails if bypassed.
        /// </summary>
        [TestMethod]
        public void QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine()
        {
            // Arrange
            var (controller, groups) = BuildLooseMetricsController();
            SetPrivateField(controller, "_stopWatchMoved", new Stopwatch());
            var fake = FixedClock();
            controller.TimeProvider = fake;
            var expectedLocal = fake.GetLocalNow().LocalDateTime;
            var expectedDataLineBeg =
                expectedLocal.ToString("MM/dd/yyyy") + "," + expectedLocal.ToString("hh:mm") + ",";

            // Act
            controller.QuickFileMetrics_WRITE("metrics.csv");

            // Assert
            groups.Verify(
                x =>
                    x.GetMoveDiagnostics(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<double>(),
                        expectedDataLineBeg,
                        expectedLocal,
                        ref It.Ref<AppointmentItem>.IsAny
                    ),
                Times.Once
            );
        }

        #endregion Issue #222 — Injectable time/delay seam tests

        #region Issue #442 — metrics flush tests

        private const string FakeDocumentsRoot = @"C:\FakeDocs";

        private sealed class MetricsWrite
        {
            internal MetricsWrite(
                string filename,
                string[] lines,
                string folderRoot,
                CancellationToken token
            )
            {
                Filename = filename;
                Lines = lines;
                FolderRoot = folderRoot;
                Token = token;
            }

            internal string Filename { get; }
            internal string[] Lines { get; }
            internal string FolderRoot { get; }
            internal CancellationToken Token { get; }
        }

        /// <summary>
        /// The flush must actually reach the writer. On the pre-fix source the diagnostic lines are
        /// handed to a queue whose consumer can never start, so the capture list stays empty.
        /// </summary>
        [TestMethod]
        public async Task WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce()
        {
            var lines = new[] { "line-one", "line-two" };
            var (controller, _) = BuildLooseMetricsController(lines);
            var captures = new List<MetricsWrite>();
            controller.MetricsFileWriter = (filename, written, folderRoot, token) =>
            {
                captures.Add(new MetricsWrite(filename, written, folderRoot, token));
                return Task.CompletedTask;
            };

            await controller.WriteMetricsAsync("metrics.csv");

            captures.Should().ContainSingle("the flush must invoke the writer exactly once");
            captures[0].Filename.Should().Be("metrics.csv");
            captures[0].FolderRoot.Should().Be(FakeDocumentsRoot);
            captures[0].Lines.Should().Equal(lines);
        }

        /// <summary>
        /// The flush-timing invariant: the writer's Task must complete before the Task returned by
        /// WriteMetricsAsync completes, with nothing deferred to a timer, a background consumer, or
        /// Cleanup(). Task.Yield is used so the delegate genuinely suspends without a wall-clock wait.
        /// </summary>
        [TestMethod]
        public async Task WriteMetricsAsync_CompletesWriterTaskBeforeReturning()
        {
            var (controller, _) = BuildLooseMetricsController(new[] { "line-one" });
            var writerCompleted = false;
            controller.MetricsFileWriter = async (filename, written, folderRoot, token) =>
            {
                await Task.Yield();
                writerCompleted = true;
            };

            await controller.WriteMetricsAsync("metrics.csv");

            writerCompleted
                .Should()
                .BeTrue("the writer must complete before WriteMetricsAsync returns");
        }

        /// <summary>
        /// The flush must survive session cancellation. The dispatcher continuation that carries it
        /// is not awaited to completion, so a cancel raised while the write is in flight must not
        /// abort it. The writer therefore receives an uncancelled token, never the session token.
        /// </summary>
        [TestMethod]
        public async Task WriteMetricsAsync_PassesUncancelledTokenToWriter()
        {
            var (controller, _) = BuildLooseMetricsController(new[] { "line-one" });
            var captured = new List<CancellationToken>();
            controller.MetricsFileWriter = (filename, written, folderRoot, token) =>
            {
                captured.Add(token);
                return Task.CompletedTask;
            };
            controller.TokenSource.Cancel();

            await controller.WriteMetricsAsync("metrics.csv");

            captured.Should().ContainSingle();
            captured[0]
                .IsCancellationRequested.Should()
                .BeFalse("a cancelled session must not abort the metrics flush");
        }

        /// <summary>
        /// GetMoveDiagnostics returns an array one element longer than it fills, so its trailing
        /// element is null. Null and whitespace-only entries must be dropped before the write rather
        /// than producing a blank CSV line.
        /// </summary>
        [TestMethod]
        public async Task WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting()
        {
            var (controller, _) = BuildLooseMetricsController(
                new[] { "line-one", "   ", null, "line-two" }
            );
            var captures = new List<MetricsWrite>();
            controller.MetricsFileWriter = (filename, written, folderRoot, token) =>
            {
                captures.Add(new MetricsWrite(filename, written, folderRoot, token));
                return Task.CompletedTask;
            };

            await controller.WriteMetricsAsync("metrics.csv");

            captures.Should().ContainSingle();
            captures[0]
                .Lines.Should()
                .Equal(
                    new[] { "line-one", "line-two" },
                    "null and whitespace-only entries must not reach the file"
                );
        }

        /// <summary>
        /// Guards the MyDocuments guard: with no MyDocuments entry the method must return before any
        /// write. This test passes both before and after the fix by design.
        /// </summary>
        [TestMethod]
        public async Task WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter()
        {
            var (controller, _) = BuildLooseMetricsController(
                new[] { "line-one" },
                withMyDocuments: false
            );
            var invoked = false;
            controller.MetricsFileWriter = (filename, written, folderRoot, token) =>
            {
                invoked = true;
                return Task.CompletedTask;
            };

            await controller.WriteMetricsAsync("metrics.csv");

            invoked
                .Should()
                .BeFalse("the guard must abort before any write when MyDocuments is absent");
        }

        #endregion Issue #442 — metrics flush tests
    }
}
