# Negative Control C1: Directory Exists Inverted (P1-T9, P1-T10)

Timestamp: 2026-09-30T07-37
Task: P1-T9 [expect-fail]
Command: CMD-CENSUS with PATH = UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false; TASKID p1-t9); vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory" "/ResultsDirectory:coverage\test-results\940\p1-t9" "/Logger:trx;LogFileName=p1-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; NAMES-PFS)
EXIT_CODE: 1 (scoped to the mutated vstest run)
ExpectedExitCode: 1
Output Summary: the mutation compiled and the predicted test failed on the predicted assertion with the predicted phrase `but found False`; total 1, failed 1.

## Mutated run

- MUTATED-FILE: UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs line 34
- PRE-EDIT-LINE: `public bool Exists => _directoryInfo.Exists;`
- MUTATED-LINE: `public bool Exists => !_directoryInfo.Exists;`
- CENSUS-TRANSITION: TOKEN !_directoryInfo.Exists = 1
- Other non-zero census lines: TOKEN using System.Linq; = 1; TOKEN .Create() = 1; TOKEN .Create( = 2; TOKEN .CreateSubdirectory( = 2; TOKEN .EnumerateDirectories( = 3; TOKEN .EnumerateFiles( = 3; TOKEN .EnumerateFileSystemInfos( = 3; TOKEN .GetDirectories( = 3; TOKEN .GetFiles( = 3; TOKEN .GetFileSystemInfos( = 3; TOKEN .GetAccessControl( = 2; TOKEN .GetObjectData( = 1; TOKEN .Refresh() = 1; TOKEN .SetAccessControl( = 1; TOKEN .ToString() = 1; TOKEN .Delete() = 1; TOKEN .MoveTo( = 1; TOKEN .Parent = 2; TOKEN .Root = 1; TOKEN .Attributes = 2; TOKEN .CreationTime = 4; TOKEN .CreationTimeUtc = 2; TOKEN .LastAccessTime = 4; TOKEN .LastAccessTimeUtc = 2; TOKEN .LastWriteTime = 4; TOKEN .LastWriteTimeUtc = 2; TOKEN _directoryInfo.FullName = 1
- LINES = 217
- SHA256 = 260648C3FBE87C0E5723B709306B3CD94154D5E1962B3C8E8716AD3E1486608F
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- RUNSETTINGS: scripts\vscode\TaskMaster.cli.runsettings with /InIsolation
- FILTER: FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=0 failed=1
- RESULT PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory = Failed
- MESSAGE PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory :: Expected adapter.Exists to be True, but found False.
- PREDICTED-PHRASE: `but found False` (present)

## Revert and confirming run

- Task: P1-T10
- Command: git checkout -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; git diff --exit-code HEAD -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; git status --porcelain -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; CMD-CENSUS (same PATH; the payload printed the non-zero TOKEN lines plus the transition token); CMD-BUILD with TASKID p1-t10; CMD-VSTEST with the C1 filter, NAMES-PFS and TASKID p1-t10
- CHECKOUT-EXIT: 0
- REVERT-DIFF-EXIT: 0
- PORCELAIN: EMPTY
- CENSUS-TRANSITION: TOKEN !_directoryInfo.Exists = 0
- SHA256: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242 (anchor PRE-EDIT-HASH-PDA: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242; equal)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- TRX_PRESENT: True; SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=1 failed=0
- RESULT PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory = Passed
