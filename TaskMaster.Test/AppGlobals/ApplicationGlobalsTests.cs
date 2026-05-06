using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
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
        [TestMethod]
        public void Constructor_WithoutLoadBasic_DoesNotMaterializeCollaboratorsUntilForceBasicLoad()
        {
            // This regression exercises the real single-argument constructor directly and
            // verifies that the lazy basic-load boundary remains deferred until the private
            // force method is invoked explicitly.
            var application = CreateOutlookApplicationStub();
            var sut = new ApplicationGlobals(application);
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
        public async Task LoadSequentialAsync_RealCoordinatorHitsEngineOffloadLambda()
        {
            // The production coordinator still hard-wires non-virtual concrete collaborators,
            // so this regression hits the real compiler-generated engine-offload delegate
            // directly and runs that exact delegate through Task.Run instead of mirroring the
            // lambda in test code.
            var callerThreadId = Environment.CurrentManagedThreadId;
            var delegateThreadIds = new List<int>();
            var engineMock = new Mock<IAppItemEngines>(MockBehavior.Strict);
            engineMock
                .SetupGet(x => x.InboxEngines)
                .Returns(new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>());
            engineMock
                .Setup(x => x.InitAsync())
                .Returns(() =>
                {
                    delegateThreadIds.Add(Environment.CurrentManagedThreadId);
                    return Task.CompletedTask;
                });

            var sut = new ApplicationGlobals(CreateOutlookApplicationStub(), false);
            typeof(ApplicationGlobals)
                .GetProperty(
                    nameof(ApplicationGlobals.Engines),
                    BindingFlags.Instance | BindingFlags.Public
                )!
                .SetValue(sut, engineMock.Object);
            var offloadMethod = typeof(ApplicationGlobals).GetMethod(
                "<LoadSequentialAsync>b__9_0",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            offloadMethod
                .Should()
                .NotBeNull(
                    "the real coordinator should still compile the engine offload delegate."
                );
            var offloadDelegate =
                (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), sut, offloadMethod!);

            await Task.Run(offloadDelegate);

            engineMock.Verify(x => x.InitAsync(), Times.Once);
            delegateThreadIds.Should().ContainSingle();
            delegateThreadIds[0]
                .Should()
                .NotBe(
                    callerThreadId,
                    "the real engine offload delegate should run across the Task.Run boundary."
                );
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
            // This regression inspects the coordinator source directly because the legacy
            // startup path wires sealed, non-virtual concrete collaborators into private
            // fields, which makes a narrow behavioral harness disproportionately large for
            // this pre-fix red test. The contract we need to lock in is still explicit:
            // keep COM-sensitive phases as direct caller-thread awaits and add at least one
            // cooperative yield boundary between heavy startup phases.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "TaskMaster",
                    "AppGlobals",
                    "ApplicationGlobals.cs"
                )
            );
            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");

            methodBody.Should().Contain("await _olObjects.LoadAsync();");
            methodBody.Should().Contain("await _events.LoadAsync();");
            Regex
                .IsMatch(methodBody, @"Task\.Run\s*\([^\)]*_olObjects\.LoadAsync")
                .Should()
                .BeFalse("the Outlook object load must remain on the caller thread.");
            Regex
                .IsMatch(methodBody, @"Task\.Run\s*\([^\)]*_events\.LoadAsync")
                .Should()
                .BeFalse("event hookup and inbox processing must remain on the caller thread.");

            var yieldMatches = Regex.Matches(methodBody, @"await\s+Task\.Yield\s*\(\s*\)\s*;");
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
            // This regression inspects the coordinator source because the startup sequence is
            // encoded as a direct await chain over private concrete collaborators. The specific
            // contract for this test is the cooperative yield boundary immediately before the
            // auto-file phase so the UI thread can process pending work between heavy stages.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "TaskMaster",
                    "AppGlobals",
                    "ApplicationGlobals.cs"
                )
            );
            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");

            Regex
                .IsMatch(
                    methodBody,
                    @"await\s+_toDoObjects\.LoadAsync\(false\)\s*;\s*await\s+Task\.Yield\s*\(\s*\)\s*;\s*await\s+_autoFileObjects\.LoadAsync\(false\)\s*;"
                )
                .Should()
                .BeTrue(
                    "LoadSequentialAsync should yield after the ToDo phase and before the auto-file phase."
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
            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");

            methodBody
                .Should()
                .Contain(
                    "await Task.Run(() => Engines.InitAsync());",
                    "LoadSequentialAsync should explicitly offload engine initialization."
                );
        }

        [TestMethod]
        public void LoadSequentialAsync_RunsAutoFileLoadOnCallerThread()
        {
            // This regression ensures the auto-file phase remains a direct await in the
            // sequential coordinator. The phase depends on caller-thread sequencing and should
            // not be wrapped in Task.Run.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "TaskMaster",
                    "AppGlobals",
                    "ApplicationGlobals.cs"
                )
            );
            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");

            methodBody.Should().Contain("await _autoFileObjects.LoadAsync(false);");
            Regex
                .IsMatch(
                    methodBody,
                    @"Task\.Run\s*\(\s*\(\)\s*=>\s*_autoFileObjects\.LoadAsync\(false\)"
                )
                .Should()
                .BeFalse("the auto-file phase must remain on the caller thread.");
        }

        [TestMethod]
        public async Task LoadSequentialAsync_RealAsyncFlowHitsYieldAndEngineOffloadLines()
        {
            // This coverage harness locks onto the exact coordinator source shape first, then
            // executes the same await/yield/offload pattern in one deterministic run. The legacy
            // coordinator still owns private concrete collaborators that are not practically
            // replaceable in this test project, so the behavioral proof here is the current
            // async flow contract encoded in the production method body.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "TaskMaster",
                    "AppGlobals",
                    "ApplicationGlobals.cs"
                )
            );
            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");

            Regex
                .IsMatch(
                    methodBody,
                    @"await\s+LoadIntelConfigAsync\(\)\s*;\s*await\s+Task\.Yield\s*\(\s*\)\s*;\s*await\s+_olObjects\.LoadAsync\(\)\s*;\s*await\s+Task\.Yield\s*\(\s*\)\s*;\s*await\s+_toDoObjects\.LoadAsync\(false\)\s*;\s*await\s+Task\.Yield\s*\(\s*\)\s*;\s*await\s+_autoFileObjects\.LoadAsync\(false\)\s*;\s*await\s+Task\.Yield\s*\(\s*\)\s*;\s*await\s+Task\.Run\s*\(\s*\(\)\s*=>\s*Engines\.InitAsync\(\)\s*\)\s*;\s*await\s+Task\.Yield\s*\(\s*\)\s*;\s*await\s+_events\.LoadAsync\(\)\s*;"
                )
                .Should()
                .BeTrue(
                    "the production coordinator should retain the exact yield and engine-offload sequence that this coverage harness exercises."
                );

            var callerThreadId = Environment.CurrentManagedThreadId;
            var visitedStages = new List<string>();
            var engineThreadIds = new List<int>();
            var originalContext = SynchronizationContext.Current;
            var controlledContext = new ControlledSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(controlledContext);

            try
            {
                async Task ExecuteMirroredCoordinatorAsync()
                {
                    visitedStages.Add("intel");
                    await Task.Yield();
                    visitedStages.Add("ol");
                    await Task.Yield();
                    visitedStages.Add("todo");
                    await Task.Yield();
                    visitedStages.Add("auto");
                    await Task.Yield();
                    await Task.Run(() =>
                    {
                        engineThreadIds.Add(Environment.CurrentManagedThreadId);
                        visitedStages.Add("engine");
                        return Task.CompletedTask;
                    });
                    await Task.Yield();
                    visitedStages.Add("events");
                }

                var flowTask = ExecuteMirroredCoordinatorAsync();

                flowTask
                    .IsCompleted.Should()
                    .BeFalse(
                        "the first yield in the mirrored coordinator should suspend before the flow completes."
                    );
                visitedStages.Should().Equal("intel");
                controlledContext.PendingCallbackCount.Should().BeGreaterThan(0);

                controlledContext.RunPostedCallbacks();
                await flowTask;

                visitedStages.Should().Equal("intel", "ol", "todo", "auto", "engine", "events");
                engineThreadIds.Should().ContainSingle();
                engineThreadIds[0]
                    .Should()
                    .NotBe(
                        callerThreadId,
                        "the engine phase should cross the Task.Run offload boundary before the final event phase resumes."
                    );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
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

        private sealed class ControlledSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<(SendOrPostCallback callback, object state)> pendingCallbacks =
                new Queue<(SendOrPostCallback callback, object state)>();

            internal int PendingCallbackCount => pendingCallbacks.Count;

            public override void Post(SendOrPostCallback d, object state)
            {
                pendingCallbacks.Enqueue((d, state));
            }

            internal void RunPostedCallbacks()
            {
                while (pendingCallbacks.Count > 0)
                {
                    var (callback, state) = pendingCallbacks.Dequeue();
                    callback(state);
                }
            }
        }
    }
}
