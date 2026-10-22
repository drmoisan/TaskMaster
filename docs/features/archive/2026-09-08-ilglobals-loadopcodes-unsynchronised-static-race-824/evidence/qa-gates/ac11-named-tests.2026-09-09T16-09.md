# AC11 named tests, with discovery-count control (Issue #824, task P5-T9)

Timestamp: 2026-09-09T16-09

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $vsw = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"; $vstest = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~ILGlobals_Tests" /Logger:trx /ResultsDirectory:coverage/trx-p5t9 2>&1 | Tee-Object -FilePath coverage/test-p5t9.log | Select-Object -Last 5'`

EXIT_CODE: 0

## Output Summary

TRX selection, per plan D15:

- TRX files found in `coverage/trx-p5t9`: **1**
- Selected TRX `LastWriteTime`: `2026-09-09T15-35-10`

The `Sort-Object LastWriteTime -Descending` stage is load-bearing rather than an optimisation: this
results directory would accumulate a second TRX on any Phase 5 loop restart, vstest never
overwrites, and directory enumeration is filename-ordered, so an unsorted selection could read a
superseded run. On this pass only one file was present, so the selection is unambiguous.

Counts read from the selected TRX:

```
TOTAL=14
PASSED=14
FAILED=0
```

## The six named tests

All present in the run's test list, all with outcome `Passed`:

| Test | Outcome | Criterion |
|---|---|---|
| `LoadOpCodes_DoesNotRepublishPublishedTables` | Passed | AC2 |
| `SingleByteOpCodes_FieldIsInitOnly` | Passed | AC3 |
| `MultiByteOpCodes_FieldIsInitOnly` | Passed | AC3 |
| `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` | Passed | AC4 |
| `SingleByteOpCodes_IsPublishedWithFullLength` | Passed | AC7 |
| `MultiByteOpCodes_IsPublishedWithFullLength` | Passed | AC7 |

The remaining eight, all `Passed`: `ProcessSpecialTypes_SystemString_ReturnsString`,
`ProcessSpecialTypes_SystemDotstring_ReturnsString`, `ProcessSpecialTypes_StringAlone_ReturnsString`,
`ProcessSpecialTypes_SystemInt32_ReturnsInt`, `ProcessSpecialTypes_Int32_ReturnsInt`,
`ProcessSpecialTypes_Int_ReturnsInt`, `ProcessSpecialTypes_UnknownType_ReturnsSameString`,
`Cache_IsInitialized`.

## Why the name list is paired with the total

`TOTAL=14` is the discovery-count control, and pairing it with the name list is what distinguishes
"the test passed" from "the test never ran". A filter that discovered nothing, or that discovered a
subset, would report a different total; a name list alone could not detect a test silently dropped
from discovery, and a total alone could not confirm which tests it counted.

The value 14 matches the P3-T5 measurement taken before the format pass, so the P5-T1 formatter
rewrite changed no test's discoverability.

Console summary:

```
Test Run Successful.
Total tests: 14
     Passed: 14
 Total time: 1.3307 Seconds
```

AC11 step 4 requires zero failed tests and the tests named in AC2, AC3, AC4 and AC7 present in the
run's test list. Both hold: `FAILED=0`, and all six are listed above. The full-suite figure of 7212
passed and 0 failed is recorded separately in
`evidence/qa-gates/coverage-post-change.2026-09-09T16-08.md`.

The TRX file carries host tokens and stays under `coverage/`, which is gitignored. It is not
committed.
