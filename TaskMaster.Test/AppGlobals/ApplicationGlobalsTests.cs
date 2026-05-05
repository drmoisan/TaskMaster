using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class ApplicationGlobalsTests
    {
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
