# P2-T7 Scoped CSharpier Format and Check

Timestamp: 2026-09-29T09-16
ITERATION: 1
Command: (1) Get-FileHash -Algorithm SHA256 of the five Write Set .cs files; dotnet tool run csharpier format QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs; Get-FileHash again (2) dotnet tool run csharpier check with the same five paths (3) CMD-CENSUS on each of the five files
EXIT_CODE: 0

Output Summary:
- EXIT_CODE is scoped to the csharpier check invocation. FORMAT_EXIT: 0.
- Format console line: Formatted 5 files in 4177ms. (processed-file count, not REWRITTEN)
- check final summary line verbatim: Checked 5 files in 1950ms.
- REWRITTEN: 2 (the two new files, Part2 and DedicatedWorkerThread.cs; line counts unchanged at 202 and 48 and every token count unchanged, and both now carry CRLF endings, consistent with the end_of_line = crlf .editorconfig setting of fact 23)
- Hashes, before and after format:
  - AFF before: BAD939900B47AE0AC963D203F621D28C959CC7F3E492119F1BD8272197EF5940
  - AFF after: BAD939900B47AE0AC963D203F621D28C959CC7F3E492119F1BD8272197EF5940
  - PART2 before: FB94BB94B65EA6F3C9E634DB2E0B5219CC003C3FF0F8565DB9C746BCA302B57D
  - PART2 after: D6A35090996E48283CA04D8EE2C441F8ED8946BAECC1D06D9F16931FF767976E
  - HELPER before: 309FC3119F39B08EFE766176DC4D33C44342896B6197DB09D986F53EDBAD59C8
  - HELPER after: 986838E4FD72A7E233B26A1BE03B5912DFBB4016980F515F5C01680A0ABF688E
  - BND before: 4F8B8AFFA9387043AAE3A66EF2028D96AB4F0FA77AFCE244C9044E67B9B8E0FE
  - BND after: 4F8B8AFFA9387043AAE3A66EF2028D96AB4F0FA77AFCE244C9044E67B9B8E0FE
  - FIW before: 142BCB8E95D710890A71F325A65801ED8EB53A87A339B10018D889491ECEF596
  - FIW after: 142BCB8E95D710890A71F325A65801ED8EB53A87A339B10018D889491ECEF596
- Every post value of the Token Census Expectations table holds for all five files; every LINES value is at most 500 (AFF 294, PART2 202, HELPER 48, BND 386, FIW 357). FILE SIZE LIMIT EXCEEDED did not fire.

## Census: QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs (AFF)

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

## Census: QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs (PART2)

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

## Census: QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs (HELPER)

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

## Census: QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs (BND)

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

## Census: UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs (FIW)

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
