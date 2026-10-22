# P0-T12 Pre-Edit Census

Timestamp: 2026-09-29T09-05
Command: CMD-CENSUS once per file (pwsh payload; case-sensitive non-overlapping occurrence counts with [regex]::Matches over Get-Content -Raw; LINES = Get-Content line count; SHA256 = Get-FileHash Hash)
EXIT_CODE: 0

Output Summary:
- N1: 1 (DISP return true; count)
- N2: 1 (DISP _ownerThreadId.HasValue count; at least 1)
- Every value in the pre column of the Token Census Expectations table matches (AFF LINES 490, Task.Run( 1, RunOnDedicatedWorkerThread 5, partial class 0; BND LINES 361, Task.Run( 1, .GetAwaiter() 4, ownerThreadId 0; FIW LINES 359, GetSolutionFile 5, TaskMaster.sln 1, FileMode. 12, FileAccess. 10, FixturePath 0; CSPROJ INCLUDE-PART2 0 and INCLUDE-HELPER 0; IVB System.Threading.SynchronizationContext.Current 0).
- The four SHA256 values of the existing Write Set files equal the P0-T4 PRE-EDIT-HASH values (AFF 8EBC19F8..., BND 8A8802E4..., FIW F11AA428..., CSPROJ 2AABD5BA...). TREE DIVERGED FROM PLAN did not fire.

## QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs

    FILE QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    TOKEN Task.Run( = 1
    TOKEN .GetAwaiter() = 1
    TOKEN DedicatedWorkerThread.Run( = 0
    TOKEN RunOnDedicatedWorkerThread = 5
    TOKEN ClearViewerDispatcher( = 2
    TOKEN partial class = 0
    TOKEN [TestMethod] = 7
    TOKEN action(); = 1
    TOKEN new Thread( = 1
    TOKEN IsBackground = true = 1
    TOKEN thread.Join(); = 1
    TOKEN Join( = 4
    TOKEN Join() = 4
    TOKEN using System.Reflection; = 1
    TOKEN using QuickFiler.Test.TestSupport; = 0
    TOKEN namespace QuickFiler.Test.TestSupport = 0
    TOKEN internal static class DedicatedWorkerThread = 0
    TOKEN internal static Exception Run(Action action) = 0
    TOKEN .Should() = 23
    TOKEN dedicated worker thread must not be = 2
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
    LINES = 490
    SHA256 = 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

## QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs

    FILE QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
    TOKEN Task.Run( = 1
    TOKEN .GetAwaiter() = 4
    TOKEN DedicatedWorkerThread.Run( = 0
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
    TOKEN using QuickFiler.Test.TestSupport; = 0
    TOKEN namespace QuickFiler.Test.TestSupport = 0
    TOKEN internal static class DedicatedWorkerThread = 0
    TOKEN internal static Exception Run(Action action) = 0
    TOKEN .Should() = 32
    TOKEN dedicated worker thread must not be = 0
    TOKEN distinct from every live thread by construction = 0
    TOKEN unconditionally = 0
    TOKEN owner.CheckAccess() = 0
    TOKEN .NotBe( = 0
    TOKEN ownerThreadId = 0
    TOKEN .BeNull( = 0
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
    LINES = 361
    SHA256 = 8A8802E4855FAFBFC7734F6BA4F0F050E44F5D0B395AC095C236693A2518420C

## UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs

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
    TOKEN .Should() = 52
    TOKEN dedicated worker thread must not be = 0
    TOKEN distinct from every live thread by construction = 0
    TOKEN unconditionally = 0
    TOKEN owner.CheckAccess() = 0
    TOKEN .NotBe( = 0
    TOKEN ownerThreadId = 0
    TOKEN .BeNull( = 0
    TOKEN executions.Should().Be(0) = 0
    TOKEN cannot marshal = 0
    TOKEN GetSolutionFile = 5
    TOKEN TaskMaster.sln = 1
    TOKEN AppDomain = 1
    TOKEN File.Exists( = 1
    TOKEN File.Create = 0
    TOKEN File.WriteAll = 0
    TOKEN File.Delete = 0
    TOKEN Path.GetTemp = 0
    TOKEN FileMode. = 12
    TOKEN FileMode.Open = 12
    TOKEN FileAccess. = 10
    TOKEN FileAccess.Read = 10
    TOKEN FileShare.ReadWrite = 8
    TOKEN Assembly.Location = 6
    TOKEN FixturePath = 0
    TOKEN using var sentinel = new FileStream( = 0
    TOKEN BeSameAs(sentinel) = 0
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
    LINES = 359
    SHA256 = F11AA4284D370C78334B20C535F530A343006D9B83406ADC82D39CA1D25A01D3

## QuickFiler.Test/QuickFiler.Test.csproj

    FILE QuickFiler.Test/QuickFiler.Test.csproj
    TOKEN Task.Run( = 0
    TOKEN .GetAwaiter() = 0
    TOKEN DedicatedWorkerThread.Run( = 0
    TOKEN RunOnDedicatedWorkerThread = 0
    TOKEN ClearViewerDispatcher( = 0
    TOKEN partial class = 0
    TOKEN [TestMethod] = 0
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
    TOKEN .Should() = 0
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
    TOKEN Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.cs" = 1
    TOKEN Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.Part2.cs" = 0
    TOKEN Include="TestSupport\DedicatedWorkerThread.cs" = 0
    TOKEN Thread.Sleep = 0
    TOKEN Task.Delay = 0
    TOKEN [Timeout = 0
    TOKEN DoNotParallelize = 0
    TOKEN Retry = 1
    LINES = 568
    SHA256 = 2AABD5BA7DAEE23D0BC656CE160E8298E9E658D8313B6C8FFCB0E31345AE2B4B

## QuickFiler/Viewers/BreadcrumbUiDispatcher.cs

    FILE QuickFiler/Viewers/BreadcrumbUiDispatcher.cs
    TOKEN Task.Run( = 0
    TOKEN .GetAwaiter() = 0
    TOKEN DedicatedWorkerThread.Run( = 0
    TOKEN RunOnDedicatedWorkerThread = 0
    TOKEN ClearViewerDispatcher( = 0
    TOKEN partial class = 0
    TOKEN [TestMethod] = 0
    TOKEN action(); = 2
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
    TOKEN .Should() = 0
    TOKEN dedicated worker thread must not be = 0
    TOKEN distinct from every live thread by construction = 0
    TOKEN unconditionally = 0
    TOKEN owner.CheckAccess() = 0
    TOKEN .NotBe( = 0
    TOKEN ownerThreadId = 7
    TOKEN .BeNull( = 0
    TOKEN executions.Should().Be(0) = 0
    TOKEN cannot marshal = 2
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
    TOKEN _ownerThreadId.HasValue = 1
    TOKEN System.Threading.SynchronizationContext.Current = 0
    TOKEN Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.cs" = 0
    TOKEN Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.Part2.cs" = 0
    TOKEN Include="TestSupport\DedicatedWorkerThread.cs" = 0
    TOKEN Thread.Sleep = 0
    TOKEN Task.Delay = 0
    TOKEN [Timeout = 0
    TOKEN DoNotParallelize = 0
    TOKEN Retry = 0
    LINES = 285
    SHA256 = 0764D49C8747276722853BF30FE32ACA133CB19A3D634A9CDA351217FD49017E

## QuickFiler/Viewers/ItemViewer.Breadcrumb.cs

    FILE QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
    TOKEN Task.Run( = 0
    TOKEN .GetAwaiter() = 0
    TOKEN DedicatedWorkerThread.Run( = 0
    TOKEN RunOnDedicatedWorkerThread = 0
    TOKEN ClearViewerDispatcher( = 0
    TOKEN partial class = 1
    TOKEN [TestMethod] = 0
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
    TOKEN .Should() = 0
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
    LINES = 460
    SHA256 = 928E466A8C4C0D69E1CDF1577E3BF42BBC7E86BB6AA88C21B06F7A96C2F12CCF

