# Negative Control C2: Directory GetFiles Returns Empty (P1-T11, P1-T12)

Timestamp: 2026-09-30T07-39
Task: P1-T11 [expect-fail]
Command: CMD-CENSUS with PATH = UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs (the payload printed the non-zero TOKEN lines plus the transition token); msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false; TASKID p1-t11); vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries" "/ResultsDirectory:coverage\test-results\940\p1-t11" "/Logger:trx;LogFileName=p1-t11.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; NAMES-PFS)
EXIT_CODE: 1 (scoped to the mutated vstest run)
ExpectedExitCode: 1
Output Summary: the mutation compiled and the predicted test failed on the first assertion over `files` with the predicted phrase `to contain "UtilitiesCS.Test.dll"`; total 1, failed 1.

## Mutated run

- MUTATED-FILE: UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs lines 160 to 161
- PRE-EDIT-LINES: `public IFileInfo[] GetFiles() =>` / `_directoryInfo.GetFiles().Select(file => new FileInfoWrapper(file)).ToArray();`
- MUTATED-LINE: `public IFileInfo[] GetFiles() => Array.Empty<IFileInfo>();`
- CENSUS-TRANSITION: TOKEN Array.Empty<IFileInfo>() = 1
- Other non-zero census lines: TOKEN using System.Linq; = 1; TOKEN .Create() = 1; TOKEN .Create( = 2; TOKEN .CreateSubdirectory( = 2; TOKEN .EnumerateDirectories( = 3; TOKEN .EnumerateFiles( = 3; TOKEN .EnumerateFileSystemInfos( = 3; TOKEN .GetDirectories( = 3; TOKEN .GetFiles( = 2; TOKEN .GetFileSystemInfos( = 3; TOKEN .GetAccessControl( = 2; TOKEN .GetObjectData( = 1; TOKEN .Refresh() = 1; TOKEN .SetAccessControl( = 1; TOKEN .ToString() = 1; TOKEN .Delete() = 1; TOKEN .MoveTo( = 1; TOKEN .Parent = 2; TOKEN .Root = 1; TOKEN .Attributes = 2; TOKEN .CreationTime = 4; TOKEN .CreationTimeUtc = 2; TOKEN .LastAccessTime = 4; TOKEN .LastAccessTimeUtc = 2; TOKEN .LastWriteTime = 4; TOKEN .LastWriteTimeUtc = 2; TOKEN _directoryInfo.FullName = 1
- LINES = 216
- SHA256 = 115AF5E375CC95772134C9EF714EE3BF46727BB47BB9DF9D6FE57B5D605272C0
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- RUNSETTINGS: scripts\vscode\TaskMaster.cli.runsettings with /InIsolation
- FILTER: FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=0 failed=1
- RESULT PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries = Failed
- MESSAGE PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries :: Expected files.Select(item => item.Name) {empty} to contain "UtilitiesCS.Test.dll".
- PREDICTED-PHRASE: `to contain "UtilitiesCS.Test.dll"` (present)

## Revert and confirming run

- Task: P1-T12
- Command: git checkout -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; git diff --exit-code HEAD -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; git status --porcelain -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; CMD-CENSUS (same PATH; non-zero TOKEN lines plus the transition token printed); CMD-BUILD with TASKID p1-t12; CMD-VSTEST with the C2 filter, NAMES-PFS and TASKID p1-t12
- CHECKOUT-EXIT: 0
- REVERT-DIFF-EXIT: 0
- PORCELAIN: EMPTY
- CENSUS-TRANSITION: TOKEN Array.Empty<IFileInfo>() = 0
- SHA256: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242 (anchor PRE-EDIT-HASH-PDA: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242; equal)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- TRX_PRESENT: True; SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=1 failed=0
- RESULT PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries = Passed
