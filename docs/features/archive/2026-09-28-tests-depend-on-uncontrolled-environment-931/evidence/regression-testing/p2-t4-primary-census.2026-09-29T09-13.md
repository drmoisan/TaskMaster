# P2-T4 Primary Affinity File Census

Timestamp: 2026-09-29T09-13
Command: CMD-CENSUS with PATH = QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs; git diff --numstat HEAD -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs; git status --porcelain -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
EXIT_CODE: 0

Output Summary:
- The five Target Source C edits applied bottom-up with the Edit tool (no PreToolUse refusal): (1) pre-edit lines 357 to 404 deleted; (2) pre-edit lines 199 to 347 deleted; (3) line 30 declaration made partial; (4) two remark lines inserted after line 27; (5) line 4 using System.Reflection; deleted.
- partial class 1 and [TestMethod] 4 - HOLD.
- Task.Run( 0, .GetAwaiter() 0, RunOnDedicatedWorkerThread 0, ClearViewerDispatcher( 0, action(); 0, new Thread( 0, IsBackground = true 0, thread.Join(); 0, using System.Reflection; 0, dedicated worker thread must not be 0, distinct from every live thread by construction 0, DedicatedWorkerThread.Run( 0 - HOLD.
- LINES 294 (at most 500 and at least 280) - HOLDS.
- numstat 3 added (at most 4) and 199 deleted (at least 197) - HOLDS.
- porcelain begins " M" - HOLDS.

## Census

    FILE QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    TOKEN Task.Run( = 0
    TOKEN .GetAwaiter() = 0
    TOKEN DedicatedWorkerThread.Run( = 0
    TOKEN RunOnDedicatedWorkerThread = 0
    TOKEN ClearViewerDispatcher( = 0
    TOKEN partial class = 1
    TOKEN [TestMethod] = 4
    TOKEN action(); = 0
    TOKEN new Thread( = 0
    TOKEN IsBackground = true = 0
    TOKEN thread.Join(); = 0
    TOKEN Join( = 0
    TOKEN Join() = 0
    TOKEN using System.Reflection; = 0
    TOKEN using QuickFiler.Test.TestSupport; = 0
    TOKEN namespace QuickFiler.Test.TestSupport = 0
    TOKEN internal static class DedicatedWorkerThread = 0
    TOKEN internal static Exception Run(Action action) = 0
    TOKEN .Should() = 10
    TOKEN dedicated worker thread must not be = 0
    TOKEN distinct from every live thread by construction = 0
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
    LINES = 294
    SHA256 = BAD939900B47AE0AC963D203F621D28C959CC7F3E492119F1BD8272197EF5940

## numstat

    3	199	QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs

## porcelain

     M QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
