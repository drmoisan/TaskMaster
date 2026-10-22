# AC3 fail-before evidence (Issue #824, task P1-T6, [expect-fail])

Timestamp: 2026-09-09T15-30

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $vsw = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"; $vstest = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~ILGlobals_Tests" /Logger:trx /ResultsDirectory:coverage/trx-p1t6 2>&1 | Tee-Object -FilePath coverage/test-p1t6.log | Select-Object -Last 8'`

ExpectedExitCode: 1

EXIT_CODE: 1

## Output Summary

TRX selection, per plan D15:

- TRX files found in `coverage/trx-p1t6`: **1**
- Selected TRX `LastWriteTime`: `2026-09-09T15-14-28`

Counts read from the selected TRX:

```
TOTAL=15
PASSED=12
FAILED=3
```

Per-test list:

| Test | Outcome |
|---|---|
| `SingleByteOpCodes_FieldIsInitOnly` | **Failed** |
| `MultiByteOpCodes_FieldIsInitOnly` | **Failed** |
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

`TOTAL=15` is the discovery-count control: 12 at baseline, plus one added by P1-T1 and two added by
P1-T4.

The third failure is `LoadOpCodes_DoesNotRepublishPublishedTables`, the AC2 gate introduced by
P1-T1. It is still red at this point by design, because the fix does not land until Phase 2. It is
recorded as such rather than treated as a defect.

Console summary:

```
Total tests: 15
     Passed: 12
     Failed: 3
Test Run Failed.
 Total time: 1.4460 Seconds
```

Both AC3 tests fail on the unfixed tree because `ILGlobals.cs:117-118` declare plain mutable fields,
so `FieldInfo.IsInitOnly` is false. This is the fail-before half of AC3's fail-before / pass-after
pair. The pass-after half is recorded by P2-T5.

The TRX file carries host tokens and stays under `coverage/`, which is gitignored. It is not
committed.
