# DIW Edit Census (P1-T4)

Timestamp: 2026-09-30T07-31
Task: P1-T4
Command: six Target Source B edits to UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs with the Edit tool, bottom-up (1 delete the helper 374 to 391; 2 line 86; 3 lines 64 to 80; 4 lines 43 to 61; 5 line 30; 6 insert the fixture block after line 15); CMD-CENSUS with PATH = UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs; git diff --numstat HEAD -- UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs; git status --porcelain -- UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs
EXIT_CODE: 0 (scoped to the CMD-CENSUS payload; the numstat and porcelain spans also exited 0)
Output Summary: all six acceptance clauses met. `GetRepositoryRoot`, `TaskMaster.sln`, `AppDomain`, `File.Exists(`, `catch`, `GetSolutionFile`, `MissingOwnedPath(` 0; `[TestMethod]` 8; `RootedFixturePath` 3; `Assembly.Location` 1; `OwnedAssemblyFile` 4; `OwnedAssemblyDirectory` 3; `using Moq;` 1; `using System.Linq;` 1; `new DirectoryInfo(` 2 = `new DirectoryInfo(RootedFixturePath)` 2; `new FileInfo(` 1 = `new FileInfo(typeof(` 1; `.GetFiles(` 7, `.GetDirectories(` 7, `.EnumerateFileSystemInfos(` 8, `.ToString()` 4; nine determinism tokens 0; numstat 32 added, 31 deleted; LINES 394; porcelain ` M`.

## Census

TOKEN GetRepositoryRoot = 0
TOKEN GetSolutionFile = 0
TOKEN TaskMaster.sln = 0
TOKEN AppDomain = 0
TOKEN File.Exists( = 0
TOKEN catch (IOException) = 0
TOKEN catch = 0
TOKEN Assembly.Location = 1
TOKEN RootedFixturePath = 3
TOKEN OwnedAssemblyFile = 4
TOKEN OwnedAssemblyDirectory = 3
TOKEN MissingOwnedPath( = 0
TOKEN __940_missing_ = 0
TOKEN Exists.Should().BeFalse() = 0
TOKEN new DirectoryInfo( = 2
TOKEN new DirectoryInfo(RootedFixturePath) = 2
TOKEN new DirectoryInfo(MissingOwnedPath( = 0
TOKEN new FileInfo( = 1
TOKEN new FileInfo(MissingOwnedPath( = 0
TOKEN new FileInfo(typeof( = 1
TOKEN [TestMethod] = 8
TOKEN using System.Linq; = 1
TOKEN using Moq; = 1
TOKEN Thread.Sleep = 0
TOKEN Task.Delay = 0
TOKEN [Timeout = 0
TOKEN DoNotParallelize = 0
TOKEN Retry = 0
TOKEN Path.GetTemp = 0
TOKEN File.Create = 0
TOKEN File.WriteAll = 0
TOKEN Directory.CreateDirectory = 0
TOKEN FileShare.ReadWrite = 0
TOKEN FileShare.None = 0
TOKEN .Create() = 2
TOKEN .Create( = 4
TOKEN .CreateSubdirectory( = 4
TOKEN .EnumerateDirectories( = 6
TOKEN .EnumerateFiles( = 6
TOKEN .EnumerateFileSystemInfos( = 8
TOKEN .GetDirectories( = 7
TOKEN .GetFiles( = 7
TOKEN .GetFileSystemInfos( = 6
TOKEN .GetAccessControl( = 4
TOKEN .GetObjectData( = 2
TOKEN .Refresh() = 2
TOKEN .SetAccessControl( = 2
TOKEN .ToString() = 4
TOKEN .Delete() = 2
TOKEN .Delete(recursive: true) = 1
TOKEN .MoveTo( = 2
TOKEN .CopyTo( = 0
TOKEN .Replace( = 0
TOKEN .Open( = 0
TOKEN .OpenRead() = 0
TOKEN .OpenText() = 0
TOKEN .AppendText() = 0
TOKEN .OpenWrite() = 0
TOKEN .Parent = 6
TOKEN .Root = 4
TOKEN .Length = 0
TOKEN .IsReadOnly = 0
TOKEN .Attributes = 4
TOKEN .CreationTime = 8
TOKEN .CreationTimeUtc = 4
TOKEN .LastAccessTime = 8
TOKEN .LastAccessTimeUtc = 4
TOKEN .LastWriteTime = 8
TOKEN .LastWriteTimeUtc = 4
TOKEN .MemberCount = 0
TOKEN Throw<FileNotFoundException>() = 0
TOKEN Throw<DirectoryNotFoundException>() = 0
TOKEN !_directoryInfo.Exists = 0
TOKEN Array.Empty<IFileInfo>() = 0
TOKEN Enumerable.Empty<IFileSystemInfo>() = 0
TOKEN set { } = 0
TOKEN public void Delete() { } = 0
TOKEN _directoryInfo.FullName = 0
TOKEN public long Length => 0; = 0
TOKEN public void MoveTo(string destFileName) { } = 0
TOKEN return string.Empty; = 0
LINES = 394
SHA256 = 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910

## Git spans

- NUMSTAT: `32	31	UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs`
- PORCELAIN: ` M UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs`
