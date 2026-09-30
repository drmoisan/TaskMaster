# Negative Control C6: File IsReadOnly Setter No-Op (P1-T19, P1-T20)

Timestamp: 2026-09-30T07-50
Task: P1-T19 [expect-fail]
Command: CMD-CENSUS with PATH = UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs (the payload printed the non-zero TOKEN lines plus the transition token); msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false; TASKID p1-t19); vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles" "/ResultsDirectory:coverage\test-results\940\p1-t19" "/Logger:trx;LogFileName=p1-t19.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; NAMES-PFS)
EXIT_CODE: 1 (scoped to the mutated vstest run)
ExpectedExitCode: 1
Output Summary: the mutation compiled and the predicted test failed on the first assertion (`setIsReadOnly`) with the predicted phrase `but no exception was thrown`; total 1, failed 1.

## Mutated run

- MUTATED-FILE: UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs line 113
- PRE-EDIT-LINE: `set => _fileInfo.IsReadOnly = value;`
- MUTATED-LINE: `set { }`
- CENSUS-TRANSITION: TOKEN set { } = 1
- Other non-zero census lines: TOKEN FileShare.None = 1; TOKEN .Create() = 1; TOKEN .Create( = 1; TOKEN .GetAccessControl( = 2; TOKEN .GetObjectData( = 1; TOKEN .Refresh() = 1; TOKEN .SetAccessControl( = 1; TOKEN .ToString() = 1; TOKEN .Delete() = 1; TOKEN .MoveTo( = 1; TOKEN .CopyTo( = 2; TOKEN .Replace( = 2; TOKEN .Open( = 1; TOKEN .OpenRead() = 1; TOKEN .OpenText() = 1; TOKEN .Length = 1; TOKEN .IsReadOnly = 1; TOKEN .Attributes = 2; TOKEN .CreationTime = 4; TOKEN .CreationTimeUtc = 2; TOKEN .LastAccessTime = 4; TOKEN .LastAccessTimeUtc = 2; TOKEN .LastWriteTime = 4; TOKEN .LastWriteTimeUtc = 2
- LINES = 183
- SHA256 = A7FFADE58D2A86306678B63BECC8AEFAB4CF35FC2444FF45DA7E0D0F1692E1F3
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- RUNSETTINGS: scripts\vscode\TaskMaster.cli.runsettings with /InIsolation
- FILTER: FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=0 failed=1
- RESULT PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles = Failed
- MESSAGE PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles :: Expected a <System.IO.FileNotFoundException> to be thrown, but no exception was thrown.
- PREDICTED-PHRASE: `but no exception was thrown` (present)

## Revert and confirming run

- Task: P1-T20
- Command: git checkout -- UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs; git diff --exit-code HEAD -- UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs; git status --porcelain -- UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs; CMD-CENSUS (same PATH; non-zero TOKEN lines plus the transition token printed); CMD-BUILD with TASKID p1-t20; CMD-VSTEST with the C6 filter, NAMES-PFS and TASKID p1-t20
- CHECKOUT-EXIT: 0
- REVERT-DIFF-EXIT: 0
- PORCELAIN: EMPTY
- CENSUS-TRANSITION: TOKEN set { } = 0
- SHA256: 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948 (anchor PRE-EDIT-HASH-PFA: 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948; equal)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- TRX_PRESENT: True; SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=1 failed=0
- RESULT PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles = Passed
