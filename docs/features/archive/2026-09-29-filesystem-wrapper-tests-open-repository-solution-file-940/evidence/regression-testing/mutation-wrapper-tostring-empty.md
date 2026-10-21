# Negative Control C11: Wrapper ToString Returns Empty (P1-T29, P1-T30)

Timestamp: 2026-09-30T08-03
Task: P1-T29 [expect-fail]
Command: CMD-CENSUS with PATH = UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs (the payload printed the non-zero TOKEN lines plus the transition token); msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false; TASKID p1-t29); vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests.ToString_ShouldDelegateToWrappedDirectoryInfo" "/ResultsDirectory:coverage\test-results\940\p1-t29" "/Logger:trx;LogFileName=p1-t29.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; NAMES-DIW)
EXIT_CODE: 1 (scoped to the mutated vstest run)
ExpectedExitCode: 1
Output Summary: the mutation compiled and the predicted test failed on its only assertion with a message containing the predicted phrase `"C:\Repo\fixture"`; total 1, failed 1.

## Mutated run

- MUTATED-FILE: UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs line 228
- PRE-EDIT-LINE: `return _directoryInfo.ToString();`
- MUTATED-LINE: `return string.Empty;`
- CENSUS-TRANSITION: TOKEN return string.Empty; = 1
- Other non-zero census lines: TOKEN using System.Linq; = 1; TOKEN .Create() = 1; TOKEN .Create( = 2; TOKEN .CreateSubdirectory( = 2; TOKEN .EnumerateDirectories( = 3; TOKEN .EnumerateFiles( = 3; TOKEN .EnumerateFileSystemInfos( = 3; TOKEN .GetDirectories( = 3; TOKEN .GetFiles( = 3; TOKEN .GetFileSystemInfos( = 3; TOKEN .GetAccessControl( = 2; TOKEN .GetObjectData( = 1; TOKEN .Refresh() = 1; TOKEN .SetAccessControl( = 1; TOKEN .Delete() = 1; TOKEN .MoveTo( = 1; TOKEN .Parent = 1; TOKEN .Root = 1; TOKEN .Attributes = 2; TOKEN .CreationTime = 4; TOKEN .CreationTimeUtc = 2; TOKEN .LastAccessTime = 4; TOKEN .LastAccessTimeUtc = 2; TOKEN .LastWriteTime = 4; TOKEN .LastWriteTimeUtc = 2; TOKEN _directoryInfo.FullName = 1
- LINES = 246
- SHA256 = A76D1936F8537422C548492A19E3F3BEBB2817B8103DEF99A75B479B74E93F32
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- RUNSETTINGS: scripts\vscode\TaskMaster.cli.runsettings with /InIsolation
- FILTER: FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests.ToString_ShouldDelegateToWrappedDirectoryInfo
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=0 failed=1
- RESULT ToString_ShouldDelegateToWrappedDirectoryInfo = Failed
- MESSAGE ToString_ShouldDelegateToWrappedDirectoryInfo :: Expected result to be "C:\Repo\fixture" with a length of 15, but "" has a length of 0, differs near "" (index 0).
- PREDICTED-PHRASE: `"C:\Repo\fixture"` (present; the rooted literal is the test's fixture constant, not a host path)

## Revert and confirming run

- Task: P1-T30
- Command: git checkout -- UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs; git diff --exit-code HEAD -- UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs; git status --porcelain -- UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs; CMD-CENSUS (same PATH; non-zero TOKEN lines plus the transition token printed); CMD-BUILD with TASKID p1-t30; CMD-VSTEST with the C11 filter, NAMES-DIW and TASKID p1-t30
- CHECKOUT-EXIT: 0
- REVERT-DIFF-EXIT: 0
- PORCELAIN: EMPTY
- CENSUS-TRANSITION: TOKEN return string.Empty; = 0
- SHA256: F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3 (anchor PRE-EDIT-HASH-DIWP: F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3; equal)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- TRX_PRESENT: True; SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=1 failed=0
- RESULT ToString_ShouldDelegateToWrappedDirectoryInfo = Passed
