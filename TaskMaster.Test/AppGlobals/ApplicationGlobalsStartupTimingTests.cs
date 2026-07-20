using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using OutlookApplication = Microsoft.Office.Interop.Outlook.Application;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class ApplicationGlobalsStartupTimingTests
    {
        private bool _originalStartupTimingEnabled;

        [TestInitialize]
        public void TestInitialize()
        {
            // Save and force the diagnostic timing flag off by default so flag-dependent tests
            // start from a known state; each test sets the value it needs explicitly.
            _originalStartupTimingEnabled = TaskMaster
                .Properties
                .Settings
                .Default
                .StartupTimingEnabled;
            TaskMaster.Properties.Settings.Default.StartupTimingEnabled = false;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TaskMaster.Properties.Settings.Default.StartupTimingEnabled =
                _originalStartupTimingEnabled;
        }

        // DoNotParallelize: these tests mutate the shared Settings.Default.StartupTimingEnabled
        // singleton and attach a memory appender to the process-global ApplicationGlobals logger,
        // so they must not run concurrently with other classes that share that global state.
        [TestMethod]
        [DoNotParallelize]
        public async Task LoadAsync_WhenTimingDisabled_RecordsNothingAndEmitsNoTable()
        {
            // Arrange: flag off (default from TestInitialize). Capture the ApplicationGlobals
            // logger output to confirm no [Startup timing] table is emitted.
            TaskMaster.Properties.Settings.Default.StartupTimingEnabled = false;
            var sut = new TestableApplicationGlobals(CreateOutlookApplicationStub());
            SetEnginesMock(sut);
            var appender = AttachMemoryAppender(typeof(ApplicationGlobals));

            try
            {
                // Act
                await sut.LoadAsync(parallel: false);

                // Assert: the no-op recorder is selected, records nothing, and emits no table.
                sut.TimingRecorder.Should().BeOfType<NullStartupTimingRecorder>();
                sut.TimingRecorder.FormatTable().Should().BeEmpty();
                var timingMessages = appender
                    .GetEvents()
                    .Select(e => e.RenderedMessage)
                    .Where(m => m != null && m.Contains("[Startup timing]"));
                timingMessages
                    .Should()
                    .BeEmpty("no startup-timing table may be emitted when the flag is off.");
            }
            finally
            {
                DetachMemoryAppender(typeof(ApplicationGlobals), appender);
            }
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrderWithLoadBasicFirst()
        {
            // Arrange: flag on.
            TaskMaster.Properties.Settings.Default.StartupTimingEnabled = true;
            var sut = new TestableApplicationGlobals(CreateOutlookApplicationStub());
            SetEnginesMock(sut);

            // Act
            await sut.LoadAsync(parallel: false);

            // Assert: the concrete recorder is selected and the recorded phase sequence is the
            // seven phases in startup order, with LoadBasic first.
            sut.TimingRecorder.Should().BeOfType<StartupTimingRecorder>();
            var recorded = ((StartupTimingRecorder)sut.TimingRecorder).RecordedPhaseNames;
            recorded.Should().HaveCount(7);
            recorded[0].Should().Be("LoadBasic", "LoadBasic must be the first recorded phase.");
            recorded
                .Should()
                .Equal(
                    "LoadBasic",
                    "IntelConfig",
                    "OlObjects",
                    "ToDo",
                    "AutoFile",
                    "Engines",
                    "Events"
                );
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task LoadAsync_WhenTimingEnabled_EmitsExactlyOneTableWithPhaseNamesAndTotal()
        {
            // Arrange: flag on, capture the ApplicationGlobals logger output.
            TaskMaster.Properties.Settings.Default.StartupTimingEnabled = true;
            var sut = new TestableApplicationGlobals(CreateOutlookApplicationStub());
            SetEnginesMock(sut);
            var appender = AttachMemoryAppender(typeof(ApplicationGlobals));

            try
            {
                // Act
                await sut.LoadAsync(parallel: false);

                // Assert: exactly one [Startup timing] emission containing each phase name and a
                // TOTAL row.
                var timingMessages = appender
                    .GetEvents()
                    .Select(e => e.RenderedMessage)
                    .Where(m => m != null && m.Contains("[Startup timing]"))
                    .ToList();
                timingMessages.Should().HaveCount(1, "the table must be emitted exactly once.");

                var table = timingMessages[0];
                table.Should().Contain("LoadBasic");
                table.Should().Contain("IntelConfig");
                table.Should().Contain("OlObjects");
                table.Should().Contain("ToDo");
                table.Should().Contain("AutoFile");
                table.Should().Contain("Engines");
                table.Should().Contain("Events");
                table.Should().Contain("TOTAL");
            }
            finally
            {
                DetachMemoryAppender(typeof(ApplicationGlobals), appender);
            }
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff()
        {
            // Arrange + Act: drive the sequential load with timing off, then on, capturing the
            // visited-stage sequence and yield count in each mode.
            var visitedOff = new List<string>();
            TaskMaster.Properties.Settings.Default.StartupTimingEnabled = false;
            var sutOff = new TestableApplicationGlobals(CreateOutlookApplicationStub(), visitedOff);
            SetEnginesMock(sutOff, visitedOff);
            await sutOff.LoadAsync(parallel: false);
            var yieldOff = sutOff.YieldCount;

            var visitedOn = new List<string>();
            TaskMaster.Properties.Settings.Default.StartupTimingEnabled = true;
            var sutOn = new TestableApplicationGlobals(CreateOutlookApplicationStub(), visitedOn);
            SetEnginesMock(sutOn, visitedOn);
            await sutOn.LoadAsync(parallel: false);
            var yieldOn = sutOn.YieldCount;

            // Assert: the visited-stage ordering and yield count are identical in both modes, so
            // instrumentation does not change functional startup behavior.
            visitedOff.Should().Equal("intel", "ol", "todo", "auto", "engine", "events");
            visitedOn.Should().Equal(visitedOff);
            yieldOn.Should().Be(yieldOff);
            yieldOn.Should().Be(5);
        }

        private static void SetEnginesMock(
            TestableApplicationGlobals sut,
            List<string> visitedStages = null
        )
        {
            var engineMock = new Mock<IAppItemEngines>(MockBehavior.Strict);
            engineMock
                .SetupGet(x => x.InboxEngines)
                .Returns(new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>());
            engineMock
                .Setup(x => x.InitAsync())
                .Returns(() =>
                {
                    visitedStages?.Add("engine");
                    return Task.CompletedTask;
                });
            typeof(ApplicationGlobals)
                .GetProperty(
                    nameof(ApplicationGlobals.Engines),
                    BindingFlags.Instance | BindingFlags.Public
                )!
                .SetValue(sut, engineMock.Object);
        }

        private static MemoryAppender AttachMemoryAppender(System.Type targetType)
        {
            var appender = new MemoryAppender();
            appender.ActivateOptions();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = (Logger)hierarchy.GetLogger(targetType.FullName);
            logger.AddAppender(appender);
            return appender;
        }

        private static void DetachMemoryAppender(System.Type targetType, MemoryAppender appender)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = (Logger)hierarchy.GetLogger(targetType.FullName);
            logger.RemoveAppender(appender);
        }

        private static OutlookApplication CreateOutlookApplicationStub()
        {
            return new Mock<OutlookApplication>().Object;
        }

        private sealed class TestableApplicationGlobals : ApplicationGlobals
        {
            // This file has no project-level <Nullable> and no whole-file #nullable pragma; the
            // pre-existing `?` annotations below need an explicit annotations context to avoid
            // CS8632. Scoping narrowly to annotations-only avoids introducing new CS86xx
            // diagnostics elsewhere in this file (no behavior change per AC7).
#nullable enable annotations
            private readonly IList<string>? _visitedStages;

            public TestableApplicationGlobals(
                OutlookApplication application,
                IList<string>? visitedStages = null
            )
#nullable restore annotations
                : base(application, false)
            {
                _visitedStages = visitedStages;
            }

            public int YieldCount { get; private set; }

            // Observation seam for the startup-timing recorder (issue #202). Reads the private
            // _timingRecorder field selected in LoadAsync so tests can assert the chosen recorder
            // type and the recorded phase sequence without a live Outlook process.
            public IStartupTimingRecorder TimingRecorder =>
                (IStartupTimingRecorder)
                    typeof(ApplicationGlobals)
                        .GetField(
                            "_timingRecorder",
                            BindingFlags.Instance | BindingFlags.NonPublic
                        )!
                        .GetValue(this)!;

            // Deterministic test seam: skip live COM collaborator construction and set a fixed
            // non-zero LoadBasic elapsed so the recorder captures a stable LoadBasic span when
            // LoadAsync drives ForceBasicLoad. The Engines mock is injected separately by tests.
            protected internal override void LoadBasicMethod()
            {
                typeof(ApplicationGlobals)
                    .GetField("_loadBasicElapsed", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(this, TimeSpan.FromMilliseconds(7));
            }

            protected internal override Task LoadIntelConfigPhaseAsync()
            {
                _visitedStages?.Add("intel");
                return Task.CompletedTask;
            }

            protected internal override async Task YieldWithContinuationProbeAsync(
                string priorPhaseName
            )
            {
                YieldCount++;
                await base.YieldWithContinuationProbeAsync(priorPhaseName);
            }

            protected internal override Task LoadOlObjectsPhaseAsync()
            {
                _visitedStages?.Add("ol");
                return Task.CompletedTask;
            }

            protected internal override Task LoadToDoPhaseAsync()
            {
                _visitedStages?.Add("todo");
                return Task.CompletedTask;
            }

            protected internal override Task LoadAutoFilePhaseAsync()
            {
                _visitedStages?.Add("auto");
                return Task.CompletedTask;
            }

            protected internal override Task LoadEventsPhaseAsync()
            {
                _visitedStages?.Add("events");
                return Task.CompletedTask;
            }

            // No-op the issue #211 Phase 3.2 host-bound diagnostics seams so the heartbeat
            // DispatcherTimer (which needs a live UiThread.Dispatcher) and the live GC.* reads never
            // execute under the unit-test seam. Mirrors the phase-wrapper override pattern above.
            protected internal override void StartStartupUiHeartbeat(
                TaskMaster.StartupDiagnosticsProbe probe
            ) { }

            protected internal override void StopStartupUiHeartbeat() { }

            protected internal override void BeginPhaseGcCapture(string phase) { }

            protected internal override void EmitPhaseGcDelta(
                TaskMaster.StartupDiagnosticsProbe probe,
                string phase
            ) { }

            // No-op the issue #211 Phase 3.6 live StoreWrapperInitClock read so LoadSequentialAsync
            // never touches the process-global accumulator under the unit-test seam (P4-T5).
            protected internal override double SampleStoreWrapperInitTotalMs() => 0.0;
        }
    }
}
