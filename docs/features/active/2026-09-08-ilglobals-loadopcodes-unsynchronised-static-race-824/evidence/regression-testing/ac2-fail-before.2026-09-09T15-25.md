# AC2 fail-before evidence (Issue #824, task P1-T3, [expect-fail])

Timestamp: 2026-09-09T15-25

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $vsw = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"; $vstest = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~ILGlobals_Tests" /Logger:trx /ResultsDirectory:coverage/trx-p1t3 2>&1 | Tee-Object -FilePath coverage/test-p1t3.log | Select-Object -Last 20'`

ExpectedExitCode: 1

EXIT_CODE: 1

## Output Summary

TRX selection, per plan D15:

- TRX files found in `coverage/trx-p1t3`: **1**
- Selected TRX `LastWriteTime`: `2026-09-09T15-11-51`

Counts read from the selected TRX:

```
TOTAL=13
PASSED=12
FAILED=1
```

Per-test list:

| Test | Outcome |
|---|---|
| `LoadOpCodes_DoesNotRepublishPublishedTables` | **Failed** |
| `LoadOpCodes_Initializes_SingleByteOpCodes` | Passed |
| `LoadOpCodes_Initializes_MultiByteOpCodes` | Passed |
| `LoadOpCodes_PopulatesKnownSingleByteOpCodes` | Passed |
| `LoadOpCodes_PopulatesKnownOpCode_Ret` | Passed |
| `ProcessSpecialTypes_SystemString_ReturnsString` | Passed |
| `ProcessSpecialTypes_SystemDotstring_ReturnsString` | Passed |
| `ProcessSpecialTypes_StringAlone_ReturnsString` | Passed |
| `ProcessSpecialTypes_SystemInt32_ReturnsInt` | Passed |
| `ProcessSpecialTypes_Int32_ReturnsInt` | Passed |
| `ProcessSpecialTypes_Int_ReturnsInt` | Passed |
| `ProcessSpecialTypes_UnknownType_ReturnsSameString` | Passed |
| `Cache_IsInitialized` | Passed |

`TOTAL=13` is the discovery-count control: the class held 12 tests at baseline and P1-T1 added one.
A run in which the named test was merely absent from a failure list would report a different total
and would not satisfy this condition.

Console summary:

```
Test Run Failed.
Total tests: 13
     Passed: 12
     Failed: 1
 Total time: 1.4390 Seconds
```

Failure site, from the captured stack trace: `ReferenceTypeAssertions.BeSameAs` raised from
`ILGlobals_Tests.LoadOpCodes_DoesNotRepublishPublishedTables()`. The failing assertion is the
`BeSameAs` reference-identity check, which is the property AC2 states, not an incidental error.

This is the fail-before half of AC2's fail-before / pass-after pair. The pass-after half is recorded
by P2-T5.

The TRX file carries host tokens and stays under `coverage/`, which is gitignored. It is not
committed.
