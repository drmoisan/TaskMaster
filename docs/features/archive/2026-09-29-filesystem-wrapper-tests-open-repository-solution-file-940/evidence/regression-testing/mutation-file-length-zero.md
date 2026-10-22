# Negative Control C5: File Length Returns Zero (P1-T17, P1-T18)

Timestamp: 2026-09-30T07-47
Task: P1-T17 [expect-fail]
Command: CMD-CENSUS with PATH = UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs (the payload printed the non-zero TOKEN lines plus the transition token); msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false; TASKID p1-t17); vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo" "/ResultsDirectory:coverage\test-results\940\p1-t17" "/Logger:trx;LogFileName=p1-t17.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; NAMES-PFS)
EXIT_CODE: 1 (scoped to the mutated vstest run)
ExpectedExitCode: 1
Output Summary: the mutation compiled and the predicted test failed on the `adapter.Length` assertion with the predicted phrase `to be greater than`; the message does not contain `because GetObjectData adds values`; total 1, failed 1.

## Mutated run

- MUTATED-FILE: UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs line 116
- PRE-EDIT-LINE: `public long Length => _fileInfo.Length;`
- MUTATED-LINE: `public long Length => 0;`
- CENSUS-TRANSITION: TOKEN public long Length => 0; = 1
- Other non-zero census lines: TOKEN FileShare.None = 1; TOKEN .Create() = 1; TOKEN .Create( = 1; TOKEN .GetAccessControl( = 2; TOKEN .GetObjectData( = 1; TOKEN .Refresh() = 1; TOKEN .SetAccessControl( = 1; TOKEN .ToString() = 1; TOKEN .Delete() = 1; TOKEN .MoveTo( = 1; TOKEN .CopyTo( = 2; TOKEN .Replace( = 2; TOKEN .Open( = 1; TOKEN .OpenRead() = 1; TOKEN .OpenText() = 1; TOKEN .IsReadOnly = 2; TOKEN .Attributes = 2; TOKEN .CreationTime = 4; TOKEN .CreationTimeUtc = 2; TOKEN .LastAccessTime = 4; TOKEN .LastAccessTimeUtc = 2; TOKEN .LastWriteTime = 4; TOKEN .LastWriteTimeUtc = 2
- LINES = 183
- SHA256 = 617AB3B49C61E5B46DCEA4FA0FD43E5A7084C10135255D5C650C7D6B0B8DF379
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- RUNSETTINGS: scripts\vscode\TaskMaster.cli.runsettings with /InIsolation
- FILTER: FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=0 failed=1
- RESULT PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo = Failed
- MESSAGE PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo :: Expected adapter.Length to be greater than 0L, but found 0L.
- PREDICTED-PHRASE: `to be greater than` (present; `because GetObjectData adds values` absent)

## Revert and confirming run

- Task: P1-T18
- Command: git checkout -- UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs; git diff --exit-code HEAD -- UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs; git status --porcelain -- UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs; CMD-CENSUS (same PATH; non-zero TOKEN lines plus the transition token printed); CMD-BUILD with TASKID p1-t18; CMD-VSTEST with the C5 filter, NAMES-PFS and TASKID p1-t18
- CHECKOUT-EXIT: 0
- REVERT-DIFF-EXIT: 0
- PORCELAIN: EMPTY
- CENSUS-TRANSITION: TOKEN public long Length => 0; = 0
- SHA256: 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948 (anchor PRE-EDIT-HASH-PFA: 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948; equal)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- TRX_PRESENT: True; SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=1 failed=0
- RESULT PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo = Passed
