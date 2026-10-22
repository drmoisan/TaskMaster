# P2-T5 Boundary-Coverage File Census

Timestamp: 2026-09-29T09-14
Command: CMD-CENSUS with PATH = QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs; git status --porcelain -- QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
EXIT_CODE: 0

Output Summary:
- Target Source D applied with the Edit tool (no PreToolUse refusal): pre-edit lines 52 to 62 replaced by the rewritten Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction; using QuickFiler.Test.TestSupport; inserted between using Moq; and using QuickFiler.Viewers;.
- Task.Run( 0 (P0-T12: 1) and .GetAwaiter() 3 (P0-T12: 4), each one less - HOLD.
- DedicatedWorkerThread.Run( 1, using QuickFiler.Test.TestSupport; 1, .NotBe( 1, .BeNull( 1, dedicated worker thread must not be 1, ownerThreadId 2 - HOLD.
- executions.Should().Be(0) 1, cannot marshal 1, partial class 1, Thread.Sleep 0, Task.Delay 0, [Timeout 0, DoNotParallelize 0 - HOLD.
- LINES 386 (at most 500) - HOLDS.
- porcelain: " M QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs".

## Census

    FILE QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
    TOKEN Task.Run( = 0
    TOKEN .GetAwaiter() = 3
    TOKEN DedicatedWorkerThread.Run( = 1
    TOKEN RunOnDedicatedWorkerThread = 0
    TOKEN ClearViewerDispatcher( = 0
    TOKEN partial class = 1
    TOKEN [TestMethod] = 5
    TOKEN action(); = 1
    TOKEN new Thread( = 0
    TOKEN IsBackground = true = 0
    TOKEN thread.Join(); = 0
    TOKEN Join( = 0
    TOKEN Join() = 0
    TOKEN using System.Reflection; = 1
    TOKEN using QuickFiler.Test.TestSupport; = 1
    TOKEN namespace QuickFiler.Test.TestSupport = 0
    TOKEN internal static class DedicatedWorkerThread = 0
    TOKEN internal static Exception Run(Action action) = 0
    TOKEN .Should() = 34
    TOKEN dedicated worker thread must not be = 1
    TOKEN distinct from every live thread by construction = 0
    TOKEN unconditionally = 0
    TOKEN owner.CheckAccess() = 0
    TOKEN .NotBe( = 1
    TOKEN ownerThreadId = 2
    TOKEN .BeNull( = 1
    TOKEN executions.Should().Be(0) = 1
    TOKEN cannot marshal = 1
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
    TOKEN return true; = 1
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
    LINES = 386
    SHA256 = 4F8B8AFFA9387043AAE3A66EF2028D96AB4F0FA77AFCE244C9044E67B9B8E0FE

## porcelain

     M QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
