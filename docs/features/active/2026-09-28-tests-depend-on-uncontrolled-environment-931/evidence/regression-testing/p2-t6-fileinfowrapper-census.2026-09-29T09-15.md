# P2-T6 FileInfoWrapper Test File Census

Timestamp: 2026-09-29T09-15
Command: CMD-CENSUS with PATH = UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs; git diff -U0 HEAD -- UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs (added-line counts computed in the same pwsh payload over lines beginning with a single +); git status --porcelain -- UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs
EXIT_CODE: 0

Output Summary:
- The three Target Source E edits applied bottom-up with the Edit tool (no PreToolUse refusal): (1) pre-edit lines 338 to 357 deleted; (2) pre-edit lines 25 to 81 replaced by the four rewritten tests; (3) the FixturePath constant with its three-line comment and one blank line inserted after the class opening brace.
- GetSolutionFile 0, TaskMaster.sln 0, AppDomain 0, File.Exists( 0, File.Create 0, File.WriteAll 0, File.Delete 0, Path.GetTemp 0 - HOLD.
- FileMode. 13 = FileMode.Open 13; FileAccess. 11 = FileAccess.Read 11 - HOLD.
- FileShare.ReadWrite 9, Assembly.Location 7, FixturePath 4, using var sentinel = new FileStream( 1, BeSameAs(sentinel) 1, .Returns(decoy) 0, wrapper.OpenRead() 2, stream.CanRead.Should().BeTrue() 1, stream.Length.Should().BeGreaterThan(0) 1, [TestMethod] 8 - HOLD.
- ADDED-LENGTH: 0 (alignment-independent definition: the line whose trimmed text is exactly stream.Length.Should().BeGreaterThan(0); is excluded; in this diff it was reported as unchanged context)
- ADDED-OPENREAD: 2 (the mock setup line and FileStream stream = wrapper.OpenRead();)
- ADDED-FIXTUREPATH: 4
- ADDED-OPEN-CREATE-WRITE: 0
- ADDED-LINE-COUNT: 24 (observation)
- LINES 357 (at most 500) - HOLDS.
- porcelain begins " M" - HOLDS.

## Census

    FILE UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs
    TOKEN Task.Run( = 0
    TOKEN .GetAwaiter() = 0
    TOKEN DedicatedWorkerThread.Run( = 0
    TOKEN RunOnDedicatedWorkerThread = 0
    TOKEN ClearViewerDispatcher( = 0
    TOKEN partial class = 0
    TOKEN [TestMethod] = 8
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
    TOKEN .Should() = 53
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
    TOKEN FileMode. = 13
    TOKEN FileMode.Open = 13
    TOKEN FileAccess. = 11
    TOKEN FileAccess.Read = 11
    TOKEN FileShare.ReadWrite = 9
    TOKEN Assembly.Location = 7
    TOKEN FixturePath = 4
    TOKEN using var sentinel = new FileStream( = 1
    TOKEN BeSameAs(sentinel) = 1
    TOKEN .Returns(decoy) = 0
    TOKEN wrapper.OpenRead() = 2
    TOKEN stream.CanRead.Should().BeTrue() = 1
    TOKEN stream.Length.Should().BeGreaterThan(0) = 1
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
    LINES = 357
    SHA256 = 142BCB8E95D710890A71F325A65801ED8EB53A87A339B10018D889491ECEF596

## Added-line counts (git diff -U0 HEAD)

    ADDED-LINE-COUNT: 24
    ADDED-LENGTH: 0
    ADDED-OPENREAD: 2
    ADDED-FIXTUREPATH: 4
    ADDED-OPEN-CREATE-WRITE: 0

## porcelain

     M UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs
