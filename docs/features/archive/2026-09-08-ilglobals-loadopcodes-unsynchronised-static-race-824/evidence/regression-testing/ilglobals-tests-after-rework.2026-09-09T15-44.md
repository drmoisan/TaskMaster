# Reworked ILGlobals_Tests run (Issue #824, task P3-T5)

Timestamp: 2026-09-09T15-44

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $vsw = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"; $vstest = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~ILGlobals_Tests" /Logger:trx /ResultsDirectory:coverage/trx-p3t5 2>&1 | Tee-Object -FilePath coverage/test-p3t5.log | Select-Object -Last 6'`

EXIT_CODE: 0

## Output Summary

TRX selection, per plan D15:

- TRX files found in `coverage/trx-p3t5`: **1**
- Selected TRX `LastWriteTime`: `2026-09-09T15-21-02`

Counts read from the selected TRX:

```
TOTAL=14
PASSED=14
FAILED=0
```

`TOTAL=14` is the discovery-count control: 12 at baseline, plus four added across P1-T1, P1-T4 and
P3-T1, minus the two deleted by P3-T3.

Per-test list, all fourteen with outcome `Passed`:

| Test | Outcome | Role |
|---|---|---|
| `LoadOpCodes_DoesNotRepublishPublishedTables` | Passed | AC2 primary gate |
| `SingleByteOpCodes_FieldIsInitOnly` | Passed | AC3 structural gate |
| `MultiByteOpCodes_FieldIsInitOnly` | Passed | AC3 structural gate |
| `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` | Passed | AC4 supporting test |
| `SingleByteOpCodes_IsPublishedWithFullLength` | Passed | AC7 renamed publication test |
| `MultiByteOpCodes_IsPublishedWithFullLength` | Passed | AC7 renamed publication test |
| `ProcessSpecialTypes_SystemString_ReturnsString` | Passed | pre-existing |
| `ProcessSpecialTypes_SystemDotstring_ReturnsString` | Passed | pre-existing |
| `ProcessSpecialTypes_StringAlone_ReturnsString` | Passed | pre-existing |
| `ProcessSpecialTypes_SystemInt32_ReturnsInt` | Passed | pre-existing |
| `ProcessSpecialTypes_Int32_ReturnsInt` | Passed | pre-existing |
| `ProcessSpecialTypes_Int_ReturnsInt` | Passed | pre-existing |
| `ProcessSpecialTypes_UnknownType_ReturnsSameString` | Passed | pre-existing |
| `Cache_IsInitialized` | Passed | pre-existing |

All six tests the plan enumerates for this task are present and passed.

Console summary:

```
Test Run Successful.
Total tests: 14
     Passed: 14
 Total time: 1.3437 Seconds
```

Note on AC4, restated per plan D2: `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` passes on the
unfixed tree as well and is a supporting test rather than a gate for this defect. Its presence in
this all-green list is not evidence that the race is fixed. That evidence is the fail-before /
pass-after pairing for AC2 and AC3.

The TRX file carries host tokens and stays under `coverage/`, which is gitignored. It is not
committed.
