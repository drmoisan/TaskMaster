using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.Threading;
using OutlookApplication = Microsoft.Office.Interop.Outlook.Application;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class ApplicationGlobalsTests
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

        [TestMethod]
        public void Constructor_WithoutLoadBasic_DoesNotMaterializeCollaboratorsUntilForceBasicLoad()
        {
            // This regression exercises the real single-argument constructor directly and
            // verifies that the lazy basic-load boundary remains deferred until the private
            // force method is invoked explicitly.
            var application = CreateOutlookApplicationStub();
            var sut = new ApplicationGlobals(
                application,
                loadBasic: false,
                readEnvironmentVariable: variable =>
                    variable == "OneDriveCommercial" ? @"C:\OneDrive" : null
            );
            var basicLoaded =
                (Lazy<bool>)
                    typeof(ApplicationGlobals)
                        .GetField("BasicLoaded", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(sut)!;

            basicLoaded.IsValueCreated.Should().BeFalse();
            typeof(ApplicationGlobals)
                .GetField("_fs", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(sut)
                .Should()
                .BeNull();
            typeof(ApplicationGlobals)
                .GetField("_olObjects", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(sut)
                .Should()
                .BeNull();
            typeof(ApplicationGlobals)
                .GetMethod("ForceBasicLoad", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(sut, null);

            basicLoaded.IsValueCreated.Should().BeTrue();
            typeof(ApplicationGlobals)
                .GetField("_fs", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(sut)
                .Should()
                .NotBeNull();
            typeof(ApplicationGlobals)
                .GetField("_olObjects", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(sut)
                .Should()
                .NotBeNull();
            sut.Engines.Should().NotBeNull();
        }

        [TestMethod]
        public async Task InitializeEnginesPhaseAsync_InvokesEngineInitializationThroughRealHelper()
        {
            // This regression executes the real helper that owns the Task.Run offload boundary
            // so the production engine-startup seam remains directly covered.
            var engineMock = new Mock<IAppItemEngines>(MockBehavior.Strict);
            engineMock
                .SetupGet(x => x.InboxEngines)
                .Returns(new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>());
            engineMock.Setup(x => x.InitAsync()).Returns(Task.CompletedTask);

            var sut = new TestableApplicationGlobals(CreateOutlookApplicationStub());
            typeof(ApplicationGlobals)
                .GetProperty(
                    nameof(ApplicationGlobals.Engines),
                    BindingFlags.Instance | BindingFlags.Public
                )!
                .SetValue(sut, engineMock.Object);

            await sut.InvokeInitializeEnginesPhaseAsync();

            engineMock.Verify(x => x.InitAsync(), Times.Once);
        }

        [TestMethod]
        public async Task LoadWhenIdle_QueuesTodoAutoFileBatchBeforeEngineAndEvents()
        {
            // This regression exercises the real LoadWhenIdle queue-registration path and
            // invokes the queued batch delegate itself so the production queued lambda body is
            // covered without replacing it with a mirrored helper.
            ResetIdleAsyncQueueState();

            var application = CreateOutlookApplicationStub();
            var engineMock = new Mock<IAppItemEngines>(MockBehavior.Strict);
            engineMock
                .SetupGet(x => x.InboxEngines)
                .Returns(new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>());
            engineMock.Setup(x => x.InitAsync()).Returns(Task.CompletedTask);
            engineMock
                .Setup(x => x.ToggleEngineAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            engineMock.Setup(x => x.EngineActiveAsync(It.IsAny<string>())).ReturnsAsync(false);
            engineMock.Setup(x => x.ShowSaveInfo(It.IsAny<string>()));
            engineMock
                .Setup(x => x.ShowDiskDialog(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            engineMock
                .Setup(x => x.RestartEngineAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            var toDoObjects = (AppToDoObjects)
                FormatterServices.GetUninitializedObject(typeof(AppToDoObjects));
            var autoFileObjects = (AppAutoFileObjects)
                FormatterServices.GetUninitializedObject(typeof(AppAutoFileObjects));
            var events = (AppEvents)FormatterServices.GetUninitializedObject(typeof(AppEvents));

            var sut = new ApplicationGlobals(application, false);
            typeof(ApplicationGlobals)
                .GetField("_toDoObjects", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(sut, toDoObjects);
            typeof(ApplicationGlobals)
                .GetField("_autoFileObjects", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(sut, autoFileObjects);
            typeof(ApplicationGlobals)
                .GetField("_events", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(sut, events);
            typeof(ApplicationGlobals)
                .GetProperty(
                    nameof(ApplicationGlobals.Engines),
                    BindingFlags.Instance | BindingFlags.Public
                )!
                .SetValue(sut, engineMock.Object);

            sut.LoadWhenIdle();

            var entries = GetIdleAsyncQueueEntries().ToArray();
            entries.Should().HaveCount(3);
            entries[0].UiThread.Should().BeFalse();
            entries[1].UiThread.Should().BeFalse();
            entries[2].UiThread.Should().BeFalse();
            entries[0]
                .AsyncAction.Method.Name.Should()
                .Contain(
                    "<LoadWhenIdle>",
                    "the first queued entry should be the real ToDo/auto-file batch delegate."
                );
            entries[1].AsyncAction.Method.Name.Should().Be(nameof(IAppItemEngines.InitAsync));
            entries[2].AsyncAction.Method.Name.Should().Be("LoadAsync");

            Func<Task> act = entries[0].AsyncAction;
            await act.Should().ThrowAsync<System.Exception>();

            GetIdleAsyncQueueEntries().Count.Should().Be(3);
        }

        [TestMethod]
        public void LoadSequentialAsync_KeepsComPhasesOnCallerThreadAndYieldsBetweenHeavyPhases()
        {
            // This regression inspects the coordinator source directly to confirm the COM-
            // sensitive phases still flow through dedicated caller-thread wrappers and that
            // cooperative yield boundaries remain between the heavy startup phases.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "TaskMaster",
                    "AppGlobals",
                    "ApplicationGlobals.cs"
                )
            );
            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");

            methodBody.Should().Contain("await LoadOlObjectsPhaseAsync();");
            methodBody.Should().Contain("await LoadEventsPhaseAsync();");
            Regex
                .IsMatch(methodBody, @"Task\.Run\s*\([^\)]*LoadOlObjectsPhaseAsync")
                .Should()
                .BeFalse("the Outlook object load phase must remain on the caller thread.");
            Regex
                .IsMatch(methodBody, @"Task\.Run\s*\([^\)]*LoadEventsPhaseAsync")
                .Should()
                .BeFalse("event hookup and inbox processing must remain on the caller thread.");

            var yieldMatches = Regex.Matches(
                methodBody,
                @"await\s+YieldWithContinuationProbeAsync\s*\([^\)]*\)\s*;"
            );
            yieldMatches
                .Count.Should()
                .BeGreaterThan(
                    0,
                    "the sequential startup coordinator should yield between heavy phases so Outlook can repaint and accept input."
                );
        }

        [TestMethod]
        public void LoadSequentialAsync_YieldsBeforeAutoFilePhase()
        {
            // This regression keeps the explicit yield boundary immediately before the auto-file
            // phase so the coordinator can pause between the ToDo and auto-file phases.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "TaskMaster",
                    "AppGlobals",
                    "ApplicationGlobals.cs"
                )
            );
            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");

            // The startup-timing instrumentation (issue #202) inserts a single per-phase
            // RecordPhase call after each phase await, so the ToDo/yield/AutoFile awaits are no
            // longer textually adjacent. The ordering guarantee is preserved: the ToDo phase is
            // awaited, then the yield boundary, then the auto-file phase, with only the timing
            // RecordPhase statement(s) interleaved. The pattern below asserts that ordering while
            // tolerating the interleaved instrumentation.
            Regex
                .IsMatch(
                    methodBody,
                    @"await\s+LoadToDoPhaseAsync\(\)\s*;[\s\S]*?await\s+YieldWithContinuationProbeAsync\([^\)]*\)\s*;[\s\S]*?await\s+LoadAutoFilePhaseAsync\(\)\s*;"
                )
                .Should()
                .BeTrue(
                    "LoadSequentialAsync should yield after the ToDo phase and before the auto-file phase."
                );
            // Guard that nothing other than timing instrumentation was interleaved between the
            // ToDo await and the yield: only a _timingRecorder.RecordPhase(...) call is allowed.
            var toDoToYield = Regex.Match(
                methodBody,
                @"await\s+LoadToDoPhaseAsync\(\)\s*;(?<between>[\s\S]*?)await\s+YieldWithContinuationProbeAsync\([^\)]*\)\s*;"
            );
            toDoToYield.Success.Should().BeTrue();
            Regex
                .IsMatch(
                    toDoToYield.Groups["between"].Value,
                    @"^\s*(_timingRecorder\.RecordPhase\([^;]*\);\s*)?$"
                )
                .Should()
                .BeTrue(
                    "only the startup-timing RecordPhase call may sit between the ToDo phase and the yield boundary."
                );
        }

        [TestMethod]
        public void LoadSequentialAsync_OffloadsEnginesInitAsyncWithTaskRun()
        {
            // This regression locks in the explicit engine offload because engine startup is a
            // heavy phase that should not monopolize the caller thread during sequential add-in
            // initialization.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "TaskMaster",
                    "AppGlobals",
                    "ApplicationGlobals.cs"
                )
            );

            source
                .Should()
                .Contain(
                    "protected internal virtual Task InitializeEnginesPhaseAsync() =>",
                    "the engine helper should remain a narrow expression-bodied offload seam."
                );
            source
                .Should()
                .Contain(
                    "Task.Run(() => Engines.InitAsync());",
                    "LoadSequentialAsync should explicitly offload engine initialization."
                );
        }

        [TestMethod]
        public void LoadSequentialAsync_RunsAutoFileLoadOnCallerThread()
        {
            // This regression ensures the auto-file phase remains a direct await in the
            // sequential coordinator helper. The phase depends on caller-thread sequencing and
            // should not be wrapped in Task.Run.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "TaskMaster",
                    "AppGlobals",
                    "ApplicationGlobals.cs"
                )
            );

            Regex
                .IsMatch(
                    source,
                    @"protected\s+internal\s+virtual\s+Task\s+LoadAutoFilePhaseAsync\s*\(\)\s*=>\s*_autoFileObjects\.LoadAsync\(false\);"
                )
                .Should()
                .BeTrue("the auto-file helper should remain a direct caller-thread await target.");
            Regex
                .IsMatch(
                    source,
                    @"protected\s+internal\s+virtual\s+Task\s+LoadAutoFilePhaseAsync\s*\(\)\s*=>\s*Task\.Run\s*\("
                )
                .Should()
                .BeFalse("the auto-file phase must remain on the caller thread.");
        }

        [TestMethod]
        public async Task LoadSequentialAsync_ExecutesRealCoordinatorSequenceThroughPhaseWrappers()
        {
            // This regression runs the real coordinator body while overriding only the phase
            // entry points that would otherwise require the full Outlook/VSTO runtime.
            var visitedStages = new List<string>();
            var engineMock = new Mock<IAppItemEngines>(MockBehavior.Strict);
            engineMock
                .SetupGet(x => x.InboxEngines)
                .Returns(new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>());
            engineMock
                .Setup(x => x.InitAsync())
                .Returns(() =>
                {
                    visitedStages.Add("engine");
                    return Task.CompletedTask;
                });
            var sut = new TestableApplicationGlobals(CreateOutlookApplicationStub(), visitedStages);
            typeof(ApplicationGlobals)
                .GetProperty(
                    nameof(ApplicationGlobals.Engines),
                    BindingFlags.Instance | BindingFlags.Public
                )!
                .SetValue(sut, engineMock.Object);

            await sut.LoadSequentialAsync();

            visitedStages.Should().Equal("intel", "ol", "todo", "auto", "engine", "events");
            sut.YieldCount.Should().Be(5);
            engineMock.Verify(x => x.InitAsync(), Times.Once);
        }

        private static string GetRepositoryRoot()
        {
            var assemblyDirectory = new DirectoryInfo(
                Path.GetDirectoryName(typeof(ThisAddIn).Assembly.Location)!
            );
            var repositoryRoot = assemblyDirectory.Parent?.Parent?.Parent?.FullName;

            repositoryRoot.Should().NotBeNullOrEmpty();
            File.Exists(Path.Combine(repositoryRoot!, "README.md")).Should().BeTrue();

            return repositoryRoot!;
        }

        private static OutlookApplication CreateOutlookApplicationStub()
        {
            return new Mock<OutlookApplication>().Object;
        }

        private static string ExtractMethodBody(string source, string methodSignature)
        {
            var signatureIndex = source.IndexOf(methodSignature, System.StringComparison.Ordinal);
            signatureIndex
                .Should()
                .BeGreaterThanOrEqualTo(0, $"source should contain '{methodSignature}'");

            var bodyStart = source.IndexOf('{', signatureIndex);
            bodyStart.Should().BeGreaterThanOrEqualTo(0, "the target method should have a body");

            var braceDepth = 0;
            for (var index = bodyStart; index < source.Length; index++)
            {
                if (source[index] == '{')
                {
                    braceDepth++;
                }
                else if (source[index] == '}')
                {
                    braceDepth--;
                    if (braceDepth == 0)
                    {
                        return source.Substring(bodyStart + 1, index - bodyStart - 1);
                    }
                }
            }

            throw new AssertFailedException($"Unable to extract body for '{methodSignature}'.");
        }

        private static void ResetIdleAsyncQueueState()
        {
            var entries = GetIdleAsyncQueueEntries();
            while (entries.TryDequeue(out _)) { }

            typeof(IdleAsyncQueue)
                .GetField("_subscribeGuard", BindingFlags.NonPublic | BindingFlags.Static)!
                .SetValue(null, new ThreadSafeSingleShotGuard());
        }

        private static ConcurrentQueue<(
            bool UiThread,
            Func<Task> AsyncAction
        )> GetIdleAsyncQueueEntries()
        {
            return (ConcurrentQueue<(bool UiThread, Func<Task> AsyncAction)>)
                typeof(IdleAsyncQueue)
                    .GetProperty("Entries", BindingFlags.NonPublic | BindingFlags.Static)!
                    .GetValue(null)!;
        }
    }
}
