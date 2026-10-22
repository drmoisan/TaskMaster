# P2-T3 Part2 Census

Timestamp: 2026-09-29T09-12
Command: CMD-CENSUS with PATH = QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs (pwsh -NoProfile -Command payload)
EXIT_CODE: 0

Output Summary:
- File created with the Target Source B content (Write tool; no PreToolUse refusal): the two moved tests copied verbatim from pre-edit lines 199 to 251 and 253 to 304 of the primary file with the remark substitutions at pre-edit lines 204 and 259 and the call substitutions at pre-edit lines 228 and 277, the rewritten null-owner test, and ClearViewerDispatcher copied verbatim from pre-edit lines 357 to 371.
- DedicatedWorkerThread.Run( 3, ClearViewerDispatcher( 2, [TestMethod] 3, dedicated worker thread must not be 3 - HOLD.
- partial class 1, using QuickFiler.Test.TestSupport; 1, using System.Reflection; 1, owner.CheckAccess() 1, .BeNull( 1, unconditionally 1 - HOLD.
- Task.Run( 0, .GetAwaiter() 0, RunOnDedicatedWorkerThread 0, action(); 0, Thread.Sleep 0, Task.Delay 0, [Timeout 0, DoNotParallelize 0, Retry 0 - HOLD.
- LINES 202 (at most 500) - HOLDS.

## Census

    FILE QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs
    TOKEN Task.Run( = 0
    TOKEN .GetAwaiter() = 0
    TOKEN DedicatedWorkerThread.Run( = 3
    TOKEN RunOnDedicatedWorkerThread = 0
    TOKEN ClearViewerDispatcher( = 2
    TOKEN partial class = 1
    TOKEN [TestMethod] = 3
    TOKEN action(); = 0
    TOKEN new Thread( = 0
    TOKEN IsBackground = true = 0
    TOKEN thread.Join(); = 0
    TOKEN Join( = 2
    TOKEN Join() = 2
    TOKEN using System.Reflection; = 1
    TOKEN using QuickFiler.Test.TestSupport; = 1
    TOKEN namespace QuickFiler.Test.TestSupport = 0
    TOKEN internal static class DedicatedWorkerThread = 0
    TOKEN internal static Exception Run(Action action) = 0
    TOKEN .Should() = 15
    TOKEN dedicated worker thread must not be = 3
    TOKEN distinct from every live thread by construction = 0
    TOKEN unconditionally = 1
    TOKEN owner.CheckAccess() = 1
    TOKEN .NotBe( = 0
    TOKEN ownerThreadId = 0
    TOKEN .BeNull( = 1
    TOKEN executions.Should().Be(0) = 0
    TOKEN cannot marshal = 0
    TOKEN GetSolutionFile = 0
    TOKEN TaskMaster.sln = 0
    TOKEN AppDomain = 0
    TOKEN File.Exists( = 0
    TOKEN File.Create = 0
    TOKEN File.WriteAll = 0
    TOKEN File.Delete = 0
    TOKEN Path.GetTemp = 0
    TOKEN FileMode. = 0
    TOKEN FileMode.Open = 0
    TOKEN FileAccess. = 0
    TOKEN FileAccess.Read = 0
    TOKEN FileShare.ReadWrite = 0
    TOKEN Assembly.Location = 0
    TOKEN FixturePath = 0
    TOKEN using var sentinel = new FileStream( = 0
    TOKEN BeSameAs(sentinel) = 0
    TOKEN .Returns(decoy) = 0
    TOKEN wrapper.OpenRead() = 0
    TOKEN stream.CanRead.Should().BeTrue() = 0
    TOKEN stream.Length.Should().BeGreaterThan(0) = 0
    TOKEN return true; = 0
    TOKEN _ownerThreadId.HasValue = 0
    TOKEN System.Threading.SynchronizationContext.Current = 0
    TOKEN Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.cs" = 0
    TOKEN Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.Part2.cs" = 0
    TOKEN Include="TestSupport\DedicatedWorkerThread.cs" = 0
    TOKEN Thread.Sleep = 0
    TOKEN Task.Delay = 0
    TOKEN [Timeout = 0
    TOKEN DoNotParallelize = 0
    TOKEN Retry = 0
    LINES = 202
    SHA256 = FB94BB94B65EA6F3C9E634DB2E0B5219CC003C3FF0F8565DB9C746BCA302B57D
