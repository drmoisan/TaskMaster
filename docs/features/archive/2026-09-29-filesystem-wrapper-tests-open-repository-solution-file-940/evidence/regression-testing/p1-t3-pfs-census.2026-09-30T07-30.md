# PFS Rewrite Census (P1-T3)

Timestamp: 2026-09-30T07-30
Task: P1-T3
Command: Write of Target Source A to UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs (Read first, then Write); CMD-CENSUS with PATH = UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs; git diff --numstat HEAD -- UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs; git status --porcelain -- UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs
EXIT_CODE: 0 (scoped to the CMD-CENSUS payload; the numstat and porcelain spans also exited 0)
Output Summary: all seven acceptance clauses met. Root-walk and catch tokens 0; `[TestMethod]` 7; `using System.Linq;` 1; `__940_missing_` 1; `MissingOwnedPath(` 10; `Exists.Should().BeFalse()` 6; `Assembly.Location` 4; `FileShare.ReadWrite` 4; `new DirectoryInfo(` 2 = 2 + 0; `new FileInfo(` 3 = 2 + 1; `Throw<FileNotFoundException>()` 20, `Throw<DirectoryNotFoundException>()` 3, `.MemberCount` 2; every at-least member token meets its bound; the nine determinism tokens 0; LINES 410; porcelain ` M`. The Write tool wrote LF line endings (git warned that LF will be replaced by CRLF); P1-T8 restores the checkout form.

## Census

TOKEN GetRepositoryRoot = 0
TOKEN GetSolutionFile = 0
TOKEN TaskMaster.sln = 0
TOKEN AppDomain = 0
TOKEN File.Exists( = 4
TOKEN catch (IOException) = 0
TOKEN catch = 0
TOKEN Assembly.Location = 4
TOKEN RootedFixturePath = 0
TOKEN OwnedAssemblyFile = 4
TOKEN OwnedAssemblyDirectory = 4
TOKEN MissingOwnedPath( = 10
TOKEN __940_missing_ = 1
TOKEN Exists.Should().BeFalse() = 6
TOKEN new DirectoryInfo( = 2
TOKEN new DirectoryInfo(RootedFixturePath) = 0
TOKEN new DirectoryInfo(MissingOwnedPath( = 2
TOKEN new FileInfo( = 3
TOKEN new FileInfo(MissingOwnedPath( = 2
TOKEN new FileInfo(typeof( = 1
TOKEN [TestMethod] = 7
TOKEN using System.Linq; = 1
TOKEN using Moq; = 0
TOKEN Thread.Sleep = 0
TOKEN Task.Delay = 0
TOKEN [Timeout = 0
TOKEN DoNotParallelize = 0
TOKEN Retry = 0
TOKEN Path.GetTemp = 0
TOKEN File.Create = 0
TOKEN File.WriteAll = 0
TOKEN Directory.CreateDirectory = 0
TOKEN FileShare.ReadWrite = 4
TOKEN FileShare.None = 0
TOKEN .Create() = 1
TOKEN .Create( = 2
TOKEN .CreateSubdirectory( = 2
TOKEN .EnumerateDirectories( = 3
TOKEN .EnumerateFiles( = 3
TOKEN .EnumerateFileSystemInfos( = 3
TOKEN .GetDirectories( = 3
TOKEN .GetFiles( = 3
TOKEN .GetFileSystemInfos( = 3
TOKEN .GetAccessControl( = 6
TOKEN .GetObjectData( = 2
TOKEN .Refresh() = 4
TOKEN .SetAccessControl( = 2
TOKEN .ToString() = 4
TOKEN .Delete() = 2
TOKEN .Delete(recursive: true) = 1
TOKEN .MoveTo( = 2
TOKEN .CopyTo( = 2
TOKEN .Replace( = 2
TOKEN .Open( = 3
TOKEN .OpenRead() = 1
TOKEN .OpenText() = 1
TOKEN .AppendText() = 1
TOKEN .OpenWrite() = 1
TOKEN .Parent = 3
TOKEN .Root = 2
TOKEN .Length = 1
TOKEN .IsReadOnly = 3
TOKEN .Attributes = 6
TOKEN .CreationTime = 12
TOKEN .CreationTimeUtc = 6
TOKEN .LastAccessTime = 12
TOKEN .LastAccessTimeUtc = 6
TOKEN .LastWriteTime = 12
TOKEN .LastWriteTimeUtc = 6
TOKEN .MemberCount = 2
TOKEN Throw<FileNotFoundException>() = 20
TOKEN Throw<DirectoryNotFoundException>() = 3
TOKEN !_directoryInfo.Exists = 0
TOKEN Array.Empty<IFileInfo>() = 0
TOKEN Enumerable.Empty<IFileSystemInfo>() = 0
TOKEN set { } = 0
TOKEN public void Delete() { } = 0
TOKEN _directoryInfo.FullName = 0
TOKEN public long Length => 0; = 0
TOKEN public void MoveTo(string destFileName) { } = 0
TOKEN return string.Empty; = 0
LINES = 410
SHA256 = 198188E24AD412CBE09ED3DCC419BA566EB20ADC7D3EFC6611BC81840F3FA2CC

## Git spans

- NUMSTAT: `215	193	UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`
- PORCELAIN: ` M UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`
