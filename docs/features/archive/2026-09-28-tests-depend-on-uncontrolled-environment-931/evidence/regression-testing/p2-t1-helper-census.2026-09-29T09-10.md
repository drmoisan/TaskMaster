# P2-T1 Helper Census

Timestamp: 2026-09-29T09-10
Command: CMD-CENSUS with PATH = QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs (pwsh -NoProfile -Command payload; case-sensitive non-overlapping occurrence counts)
EXIT_CODE: 0

Output Summary:
- File created with exactly the Target Source A content (Write tool; no PreToolUse refusal).
- Acceptance: namespace QuickFiler.Test.TestSupport 1, internal static class DedicatedWorkerThread 1, internal static Exception Run(Action action) 1, new Thread( 1, IsBackground = true 1, thread.Join(); 1, action(); 1, distinct from every live thread by construction 1 - HOLD.
- Join( 2 equals Join() 2 - HOLDS.
- .Should() 0, Task.Run( 0, Thread.Sleep 0, Task.Delay 0, [Timeout 0, DoNotParallelize 0, Retry 0 - HOLD.
- LINES 48 (at most 500) - HOLDS.

## Census

    FILE QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
    TOKEN Task.Run( = 0
    TOKEN .GetAwaiter() = 0
    TOKEN DedicatedWorkerThread.Run( = 0
    TOKEN RunOnDedicatedWorkerThread = 0
    TOKEN ClearViewerDispatcher( = 0
    TOKEN partial class = 0
    TOKEN [TestMethod] = 0
    TOKEN action(); = 1
    TOKEN new Thread( = 1
    TOKEN IsBackground = true = 1
    TOKEN thread.Join(); = 1
    TOKEN Join( = 2
    TOKEN Join() = 2
    TOKEN using System.Reflection; = 0
    TOKEN using QuickFiler.Test.TestSupport; = 0
    TOKEN namespace QuickFiler.Test.TestSupport = 1
    TOKEN internal static class DedicatedWorkerThread = 1
    TOKEN internal static Exception Run(Action action) = 1
    TOKEN .Should() = 0
    TOKEN dedicated worker thread must not be = 0
    TOKEN distinct from every live thread by construction = 1
    TOKEN unconditionally = 0
    TOKEN owner.CheckAccess() = 0
    TOKEN .NotBe( = 0
    TOKEN ownerThreadId = 0
    TOKEN .BeNull( = 0
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
    LINES = 48
    SHA256 = 309FC3119F39B08EFE766176DC4D33C44342896B6197DB09D986F53EDBAD59C8
