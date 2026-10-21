# Negative Control C10: Wrapper EnumerateFileSystemInfos Returns Empty (P1-T27, P1-T28)

Timestamp: 2026-09-30T08-00
Task: P1-T27 [expect-fail]
Command: CMD-CENSUS with PATH = UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs (the payload printed the non-zero TOKEN lines plus the transition token); msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false; TASKID p1-t27); vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests.EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles" "/ResultsDirectory:coverage\test-results\940\p1-t27" "/Logger:trx;LogFileName=p1-t27.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; NAMES-DIW)
EXIT_CODE: 1 (scoped to the mutated vstest run)
ExpectedExitCode: 1
Output Summary: the mutation compiled and the predicted test failed on the first assertion over `fileSystemInfos` with the predicted phrase `to contain "UtilitiesCS.Test.dll"`; total 1, failed 1.

## Mutated run

- MUTATED-FILE: UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs line 145
- PRE-EDIT-LINE: `return _directoryInfo.EnumerateFileSystemInfos();`
- MUTATED-LINE: `return Enumerable.Empty<IFileSystemInfo>();`
- CENSUS-TRANSITION: TOKEN Enumerable.Empty<IFileSystemInfo>() = 1
- Other non-zero census lines: TOKEN using System.Linq; = 1; TOKEN .Create() = 1; TOKEN .Create( = 2; TOKEN .CreateSubdirectory( = 2; TOKEN .EnumerateDirectories( = 3; TOKEN .EnumerateFiles( = 3; TOKEN .EnumerateFileSystemInfos( = 2; TOKEN .GetDirectories( = 3; TOKEN .GetFiles( = 3; TOKEN .GetFileSystemInfos( = 3; TOKEN .GetAccessControl( = 2; TOKEN .GetObjectData( = 1; TOKEN .Refresh() = 1; TOKEN .SetAccessControl( = 1; TOKEN .ToString() = 1; TOKEN .Delete() = 1; TOKEN .MoveTo( = 1; TOKEN .Parent = 1; TOKEN .Root = 1; TOKEN .Attributes = 2; TOKEN .CreationTime = 4; TOKEN .CreationTimeUtc = 2; TOKEN .LastAccessTime = 4; TOKEN .LastAccessTimeUtc = 2; TOKEN .LastWriteTime = 4; TOKEN .LastWriteTimeUtc = 2; TOKEN _directoryInfo.FullName = 1
- LINES = 246
- SHA256 = 376B759C1FC41D4EE6373EBA8026AB8E75520419D590F9CFC389F80A41514FF2
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- RUNSETTINGS: scripts\vscode\TaskMaster.cli.runsettings with /InIsolation
- FILTER: FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests.EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=0 failed=1
- RESULT EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles = Failed
- MESSAGE EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles :: Expected fileSystemInfos.OfType<FileInfoWrapper>().Select(item => item.Name) {empty} to contain "UtilitiesCS.Test.dll".
- PREDICTED-PHRASE: `to contain "UtilitiesCS.Test.dll"` (present)

## Revert and confirming run

- Task: P1-T28
- Command: git checkout -- UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs; git diff --exit-code HEAD -- UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs; git status --porcelain -- UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs; CMD-CENSUS (same PATH; non-zero TOKEN lines plus the transition token printed); CMD-BUILD with TASKID p1-t28; CMD-VSTEST with the C10 filter, NAMES-DIW and TASKID p1-t28
- CHECKOUT-EXIT: 0
- REVERT-DIFF-EXIT: 0
- PORCELAIN: EMPTY
- CENSUS-TRANSITION: TOKEN Enumerable.Empty<IFileSystemInfo>() = 0
- SHA256: F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3 (anchor PRE-EDIT-HASH-DIWP: F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3; equal)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- TRX_PRESENT: True; SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=1 failed=0
- RESULT EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles = Passed
