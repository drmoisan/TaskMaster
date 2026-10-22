# Negative Control C8: Directory Name Returns FullName (P1-T23, P1-T24)

Timestamp: 2026-09-30T07-55
Task: P1-T23 [expect-fail]
Command: CMD-CENSUS with PATH = UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs (the payload printed the non-zero TOKEN lines plus the transition token); msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false; TASKID p1-t23); vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests.Properties_ShouldMirrorWrappedDirectoryInfo" "/ResultsDirectory:coverage\test-results\940\p1-t23" "/Logger:trx;LogFileName=p1-t23.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; NAMES-DIW)
EXIT_CODE: 1 (scoped to the mutated vstest run)
ExpectedExitCode: 1
Output Summary: the mutation compiled and the predicted test failed on the `wrapper.Name` assertion with a message containing the predicted phrase `"fixture"`; total 1, failed 1.

## Mutated run

- MUTATED-FILE: UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs line 64
- PRE-EDIT-LINE: `public string Name => _directoryInfo.Name;`
- MUTATED-LINE: `public string Name => _directoryInfo.FullName;`
- CENSUS-TRANSITION: TOKEN _directoryInfo.FullName = 2
- Other non-zero census lines: TOKEN using System.Linq; = 1; TOKEN .Create() = 1; TOKEN .Create( = 2; TOKEN .CreateSubdirectory( = 2; TOKEN .EnumerateDirectories( = 3; TOKEN .EnumerateFiles( = 3; TOKEN .EnumerateFileSystemInfos( = 3; TOKEN .GetDirectories( = 3; TOKEN .GetFiles( = 3; TOKEN .GetFileSystemInfos( = 3; TOKEN .GetAccessControl( = 2; TOKEN .GetObjectData( = 1; TOKEN .Refresh() = 1; TOKEN .SetAccessControl( = 1; TOKEN .ToString() = 1; TOKEN .Delete() = 1; TOKEN .MoveTo( = 1; TOKEN .Parent = 2; TOKEN .Root = 1; TOKEN .Attributes = 2; TOKEN .CreationTime = 4; TOKEN .CreationTimeUtc = 2; TOKEN .LastAccessTime = 4; TOKEN .LastAccessTimeUtc = 2; TOKEN .LastWriteTime = 4; TOKEN .LastWriteTimeUtc = 2
- LINES = 217
- SHA256 = 09348328DED34860ED76CF270BBC8728F94D5E69770BABFE0F75BDCA3B08FA08
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- RUNSETTINGS: scripts\vscode\TaskMaster.cli.runsettings with /InIsolation
- FILTER: FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests.Properties_ShouldMirrorWrappedDirectoryInfo
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=0 failed=1
- RESULT Properties_ShouldMirrorWrappedDirectoryInfo = Failed
- MESSAGE (verbatim; the rooted literal is the test's fixture constant, not a host path):

```
MESSAGE Properties_ShouldMirrorWrappedDirectoryInfo :: Expected wrapper.Name to be a match with the expectation, but it differs at index 0:
    (actual)
  "C:\Repo\fixture"
  "fixture"
    (expected)
```

- PREDICTED-PHRASE: `"fixture"` (present)

## Revert and confirming run

- Task: P1-T24
- Command: git checkout -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; git diff --exit-code HEAD -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; git status --porcelain -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; CMD-CENSUS (same PATH; non-zero TOKEN lines plus the transition token printed); CMD-BUILD with TASKID p1-t24; CMD-VSTEST with the C8 filter, NAMES-DIW and TASKID p1-t24
- CHECKOUT-EXIT: 0
- REVERT-DIFF-EXIT: 0
- PORCELAIN: EMPTY
- CENSUS-TRANSITION: TOKEN _directoryInfo.FullName = 1
- SHA256: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242 (anchor PRE-EDIT-HASH-PDA: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242; equal)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- TRX_PRESENT: True; SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=1 failed=0
- RESULT Properties_ShouldMirrorWrappedDirectoryInfo = Passed
