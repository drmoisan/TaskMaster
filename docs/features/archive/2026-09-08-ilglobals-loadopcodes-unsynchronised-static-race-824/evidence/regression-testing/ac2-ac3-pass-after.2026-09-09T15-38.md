# AC2 and AC3 pass-after evidence (Issue #824, task P2-T5)

Timestamp: 2026-09-09T15-38

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $vsw = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"; $vstest = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~ILGlobals_Tests" /Logger:trx /ResultsDirectory:coverage/trx-p2t5 2>&1 | Tee-Object -FilePath coverage/test-p2t5.log | Select-Object -Last 6'`

EXIT_CODE: 0

## Output Summary

TRX selection, per plan D15:

- TRX files found in `coverage/trx-p2t5`: **1**
- Selected TRX `LastWriteTime`: `2026-09-09T15-17-52`

Counts read from the selected TRX:

```
TOTAL=15
PASSED=15
FAILED=0
```

The three gate tests, each with outcome `Passed`:

| Test | Outcome | Criterion |
|---|---|---|
| `LoadOpCodes_DoesNotRepublishPublishedTables` | Passed | AC2 |
| `SingleByteOpCodes_FieldIsInitOnly` | Passed | AC3 |
| `MultiByteOpCodes_FieldIsInitOnly` | Passed | AC3 |

The remaining twelve tests in the class also passed:
`LoadOpCodes_Initializes_SingleByteOpCodes`, `LoadOpCodes_Initializes_MultiByteOpCodes`,
`LoadOpCodes_PopulatesKnownSingleByteOpCodes`, `LoadOpCodes_PopulatesKnownOpCode_Ret`,
`ProcessSpecialTypes_SystemString_ReturnsString`,
`ProcessSpecialTypes_SystemDotstring_ReturnsString`,
`ProcessSpecialTypes_StringAlone_ReturnsString`, `ProcessSpecialTypes_SystemInt32_ReturnsInt`,
`ProcessSpecialTypes_Int32_ReturnsInt`, `ProcessSpecialTypes_Int_ReturnsInt`,
`ProcessSpecialTypes_UnknownType_ReturnsSameString`, `Cache_IsInitialized`.

Console summary:

```
Test Run Successful.
Total tests: 15
     Passed: 15
 Total time: 1.3546 Seconds
```

## Fail-before / pass-after pairing

Paired with the fail-before artifacts, this is the complete deterministic evidence for both gates:

| Criterion | Fail-before | Pass-after |
|---|---|---|
| AC2 | `evidence/regression-testing/ac2-fail-before.2026-09-09T15-25.md` — `FAILED=1`, `TOTAL=13` | this artifact — `PASSED=15`, `FAILED=0` |
| AC3 | `evidence/regression-testing/ac3-fail-before.2026-09-09T15-30.md` — `FAILED=3`, `TOTAL=15` | this artifact — `PASSED=15`, `FAILED=0` |

Both gates were red on the unfixed tree and are green on the fixed tree, with the test population
held constant at 15 across the P1-T6 and P2-T5 runs, so the transition is attributable to the fix
rather than to a change in what was discovered.

Per plan D2 this pass-after result is not presented as a demonstration that the intermittent
full-suite failure has been eliminated. It demonstrates the publication property AC2 and AC3 state.
The AC4 exhaustive-population test added in Phase 3 passes on the unfixed tree as well and is a
supporting test only.

The TRX file carries host tokens and stays under `coverage/`, which is gitignored. It is not
committed.
