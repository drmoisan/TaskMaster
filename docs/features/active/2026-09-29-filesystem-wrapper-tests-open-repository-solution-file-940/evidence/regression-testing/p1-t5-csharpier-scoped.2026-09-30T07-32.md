# Scoped CSharpier Format and Check (P1-T5)

Timestamp: 2026-09-30T07-32
Task: P1-T5
ITERATION: 1
Command: Get-FileHash -Algorithm SHA256 of the two Write Set files; dotnet tool run csharpier format UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs; Get-FileHash again; dotnet tool run csharpier check UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs; CMD-CENSUS on each file
EXIT_CODE: 0 (scoped to the `check`)
Output Summary: format exit 0 and check exit 0; one file rewritten (PFS; DIW unchanged); every post value of the Token Census Expectations table holds for both files; THROW-SUM 23; LINES 446 (PFS) and 394 (DIW), both at most 500. The hashes, format and check ran in one payload; the CMD-CENSUS of both files ran in the payload issued immediately after it, with no file write in between (the census hashes equal the after-format hashes).

- PFS-HASH-BEFORE: 198188E24AD412CBE09ED3DCC419BA566EB20ADC7D3EFC6611BC81840F3FA2CC
- PFS-HASH-AFTER: C88A785C23D8DB2960E9FAA53DF9EF91F6F00359683485BEC7EDFC44F9A2F998
- DIW-HASH-BEFORE: 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910
- DIW-HASH-AFTER: 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910
- FORMAT_EXIT_CODE: 0 (console: `Formatted 2 files in 2605ms.`, a processed-file count)
- REWRITTEN: 1
- CHECK-SUMMARY-LINE: `Checked 2 files in 1166ms.`
- CHECK_EXIT_CODE: 0
- CENSUS-TOKENS-APPENDED: NONE
- THROW-SUM: 23 (Throw<FileNotFoundException>() 20 + Throw<DirectoryNotFoundException>() 3)

## Census: UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs

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
LINES = 446
SHA256 = C88A785C23D8DB2960E9FAA53DF9EF91F6F00359683485BEC7EDFC44F9A2F998

## Census: UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs

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
